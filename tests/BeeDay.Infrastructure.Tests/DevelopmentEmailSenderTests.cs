using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

/// <summary>
/// First direct unit coverage of <see cref="DevelopmentEmailSender"/> (Epic 26, Sprint 26.1 §7
/// recorded this as a proven test-coverage gap; closed here for the capture-file mechanics this
/// sprint's plain-text alternative depends on — the content-root guard bug from Sprint 26.1 §6
/// remains a separate, already-tracked, still-open item, not addressed by this sprint).
/// </summary>
public sealed class DevelopmentEmailSenderTests : IDisposable
{
    private readonly string contentRoot = Path.Combine(Path.GetTempPath(), "beeday-dev-email-sender-tests", Guid.NewGuid().ToString("N"));

    public DevelopmentEmailSenderTests() => Directory.CreateDirectory(contentRoot);

    public void Dispose()
    {
        if (Directory.Exists(contentRoot))
        {
            Directory.Delete(contentRoot, recursive: true);
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

    private DevelopmentEmailSender CreateSender(bool enabled = true) =>
        new(
            new TestHostEnvironment { ContentRootPath = contentRoot },
            Options.Create(new DevelopmentEmailOptions { Enabled = enabled, Directory = "Data/Emails" }),
            NullLogger<DevelopmentEmailSender>.Instance);

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "BeeDay.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
