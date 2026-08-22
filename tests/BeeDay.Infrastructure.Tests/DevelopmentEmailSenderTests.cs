using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Diagnostics;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

/// <summary>
/// Direct unit coverage of <see cref="DevelopmentEmailSender"/> (first added Sprint 26.6, for the
/// capture-file mechanics the plain-text alternative depends on). Sprint 26.9 adds the tests that
/// prove the actual fix for the Sprint 26.1-proven HMG root cause (§6 of the transactional-email
/// doc): an absolute configured <c>Directory</c> now succeeds instead of throwing, while a relative
/// path attempting to escape the content root via <c>..</c> segments still fails exactly as before.
/// </summary>
public sealed class DevelopmentEmailSenderTests : IDisposable
{
    private readonly string contentRoot = Path.Combine(Path.GetTempPath(), "beeday-dev-email-sender-tests", Guid.NewGuid().ToString("N"));
    private readonly List<string> externalDirectoriesToClean = [];

    public DevelopmentEmailSenderTests() => Directory.CreateDirectory(contentRoot);

    public void Dispose()
    {
        if (Directory.Exists(contentRoot))
        {
            Directory.Delete(contentRoot, recursive: true);
        }

        foreach (var directory in externalDirectoriesToClean)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task SendAsync_WithPlainTextBody_CapturesHtmlAndPlainTextAndMetadata()
    {
        var sender = CreateSender();

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Subject", "<p>Body</p>", "Body"),
            TestContext.Current.CancellationToken);

        var directory = new DirectoryInfo(Path.Combine(contentRoot, "Data/Emails"));
        var htmlFile = Assert.Single(directory.GetFiles("*.html"));
        var textFile = Assert.Single(directory.GetFiles("*.txt"));
        var jsonFile = Assert.Single(directory.GetFiles("*.json"));

        Assert.Equal("<p>Body</p>", await File.ReadAllTextAsync(htmlFile.FullName, TestContext.Current.CancellationToken));
        Assert.Equal("Body", await File.ReadAllTextAsync(textFile.FullName, TestContext.Current.CancellationToken));

        using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(jsonFile.FullName, TestContext.Current.CancellationToken));
        Assert.Equal(htmlFile.Name, metadata.RootElement.GetProperty("HtmlFile").GetString());
        Assert.Equal(textFile.Name, metadata.RootElement.GetProperty("PlainTextFile").GetString());
    }

    [Fact]
    public async Task SendAsync_WithoutPlainTextBody_CapturesOnlyHtmlAndNullPlainTextFile()
    {
        var sender = CreateSender();

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Subject", "<p>Body</p>"),
            TestContext.Current.CancellationToken);

        var directory = new DirectoryInfo(Path.Combine(contentRoot, "Data/Emails"));
        Assert.Single(directory.GetFiles("*.html"));
        Assert.Empty(directory.GetFiles("*.txt"));

        var jsonFile = Assert.Single(directory.GetFiles("*.json"));
        using var metadata = JsonDocument.Parse(await File.ReadAllTextAsync(jsonFile.FullName, TestContext.Current.CancellationToken));
        Assert.Equal(JsonValueKind.Null, metadata.RootElement.GetProperty("PlainTextFile").ValueKind);
    }

    [Fact]
    public async Task SendAsync_WhenDisabled_CapturesNoFilesAtAll()
    {
        var sender = CreateSender(enabled: false);

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Subject", "<p>Body</p>", "Body"),
            TestContext.Current.CancellationToken);

        Assert.False(Directory.Exists(Path.Combine(contentRoot, "Data/Emails")));
    }

    // Reproduces the exact shape of the proven HMG root cause: a configured Directory that is an
    // absolute path outside the content root (appsettings.Homologation.json's real, committed
    // C:\Apps\BeeDay-Data\Emails relative to content root C:\Apps\BeeDay.Web) — chosen deliberately
    // so captured emails survive a redeploy, matching Data Protection Keys/Event Journal. Before the
    // Sprint 26.9 fix, every call with a configuration shaped like this threw before writing anything.
    [Fact]
    public async Task SendAsync_WithAbsoluteDirectoryOutsideContentRoot_Succeeds()
    {
        var externalDirectory = Path.Combine(Path.GetTempPath(), "beeday-dev-email-sender-tests-external", Guid.NewGuid().ToString("N"));
        externalDirectoriesToClean.Add(externalDirectory);
        var sender = new DevelopmentEmailSender(
            new TestHostEnvironment { ContentRootPath = contentRoot },
            Options.Create(new DevelopmentEmailOptions { Enabled = true, Directory = externalDirectory }),
            NullLogger<DevelopmentEmailSender>.Instance);

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Subject", "<p>Body</p>", "Body"),
            TestContext.Current.CancellationToken);

        Assert.Single(new DirectoryInfo(externalDirectory).GetFiles("*.html"));
    }

    // The guard's original purpose — a relative Directory must still resolve inside the content
    // root — must remain fully protected; only the absolute-path case above changed.
    [Fact]
    public async Task SendAsync_WithRelativeDirectoryEscapingContentRoot_StillThrows()
    {
        var sender = new DevelopmentEmailSender(
            new TestHostEnvironment { ContentRootPath = contentRoot },
            Options.Create(new DevelopmentEmailOptions { Enabled = true, Directory = "../../../escaped" }),
            NullLogger<DevelopmentEmailSender>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sender.SendAsync(
                new EmailMessage("player@example.com", "Subject", "<p>Body</p>"),
                TestContext.Current.CancellationToken));
    }

    // EPIC 28, Sprint 28.7: same EventId discipline as the Resend/Guard senders — captured vs.
    // suppressed must be filterable without parsing message text.
    [Fact]
    public async Task SendAsync_LogsTheCapturedEventId()
    {
        var logger = new RecordingLogger<DevelopmentEmailSender>();
        var sender = new DevelopmentEmailSender(
            new TestHostEnvironment { ContentRootPath = contentRoot },
            Options.Create(new DevelopmentEmailOptions { Enabled = true, Directory = "Data/Emails" }),
            logger);

        await sender.SendAsync(new EmailMessage("player@example.com", "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Contains(logger.Entries, e => e.EventId.Id == EmailEventIds.DevelopmentCaptured.Id);
    }

    [Fact]
    public async Task SendAsync_WhenDisabled_LogsTheCaptureDisabledEventId()
    {
        var logger = new RecordingLogger<DevelopmentEmailSender>();
        var sender = new DevelopmentEmailSender(
            new TestHostEnvironment { ContentRootPath = contentRoot },
            Options.Create(new DevelopmentEmailOptions { Enabled = false, Directory = "Data/Emails" }),
            logger);

        await sender.SendAsync(new EmailMessage("player@example.com", "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Contains(logger.Entries, e => e.EventId.Id == EmailEventIds.DevelopmentCaptureDisabled.Id);
    }

    private DevelopmentEmailSender CreateSender(bool enabled = true) =>
        new(
            new TestHostEnvironment { ContentRootPath = contentRoot },
            Options.Create(new DevelopmentEmailOptions { Enabled = enabled, Directory = "Data/Emails" }),
            NullLogger<DevelopmentEmailSender>.Instance);

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, EventId EventId, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, eventId, formatter(state, exception)));
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "BeeDay.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
