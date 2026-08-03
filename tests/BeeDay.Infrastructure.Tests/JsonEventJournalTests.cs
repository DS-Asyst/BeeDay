using System.Text.Json;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Events;
using BeeDay.Infrastructure.Auditing;
using BeeDay.Infrastructure.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class JsonEventJournalTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"beeday-journal-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task Repeated_event_id_is_written_only_once()
    {
        JsonEventJournal journal = CreateJournal();
        UserLeveledUpDomainEvent domainEvent = CreateLevelUpEvent(Guid.NewGuid());

        await journal.AppendAsync(domainEvent, TestContext.Current.CancellationToken);
        await journal.AppendAsync(domainEvent, TestContext.Current.CancellationToken);

        string[] lines = await ReadJournalLinesAsync();
        Assert.Single(lines);
    }

    [Fact]
    public async Task Different_event_for_same_experience_entry_is_written_only_once()
    {
        JsonEventJournal journal = CreateJournal();
        Guid experienceEntryId = Guid.NewGuid();

        await journal.AppendAsync(
            CreateLevelUpEvent(experienceEntryId),
            TestContext.Current.CancellationToken);
        await journal.AppendAsync(
            CreateLevelUpEvent(experienceEntryId),
            TestContext.Current.CancellationToken);

        string[] lines = await ReadJournalLinesAsync();
        Assert.Single(lines);
    }

    [Fact]
    public async Task Level_up_entry_contains_summary_and_structured_payload()
    {
        JsonEventJournal journal = CreateJournal();
        UserLeveledUpDomainEvent domainEvent = CreateLevelUpEvent(Guid.NewGuid());

        await journal.AppendAsync(domainEvent, TestContext.Current.CancellationToken);

        string line = Assert.Single(await ReadJournalLinesAsync());
        using JsonDocument document = JsonDocument.Parse(line);
        JsonElement root = document.RootElement;
        JsonElement payload = root.GetProperty("payload");

        Assert.Equal(nameof(UserLeveledUpDomainEvent), root.GetProperty("type").GetString());
        Assert.Equal(
            "Reached level 7 after gaining 20 XP from Project.",
            root.GetProperty("summary").GetString());
        Assert.Equal(domainEvent.UserId, payload.GetProperty("userId").GetGuid());
        Assert.Equal(domainEvent.ExperienceEntryId, payload.GetProperty("experienceEntryId").GetGuid());
        Assert.Equal(3, payload.GetProperty("previousLevel").GetInt32());
        Assert.Equal(7, payload.GetProperty("newLevel").GetInt32());
        Assert.Equal(4, payload.GetProperty("levelsGained").GetInt32());
        Assert.Equal("Project", payload.GetProperty("experienceSource").GetString());
    }

    [Fact]
    public async Task Uses_configured_directory_and_file_name()
    {
        IOptions<EventJournalOptions> options = Options.Create(new EventJournalOptions
        {
            Directory = "CustomAudit",
            FileName = "custom-events.ndjson",
        });
        Directory.CreateDirectory(_root);
        var journal = new JsonEventJournal(new TestHostEnvironment { ContentRootPath = _root }, options);

        await journal.AppendAsync(CreateLevelUpEvent(Guid.NewGuid()), TestContext.Current.CancellationToken);

        var path = Path.Combine(_root, "CustomAudit", "custom-events.ndjson");
        Assert.True(File.Exists(path));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private JsonEventJournal CreateJournal()
    {
        Directory.CreateDirectory(_root);
        var environment = new TestHostEnvironment
        {
            ContentRootPath = _root,
        };
        IOptions<EventJournalOptions> options = Options.Create(new EventJournalOptions());
        return new JsonEventJournal(environment, options);
    }

    private async Task<string[]> ReadJournalLinesAsync()
    {
        string path = Path.Combine(_root, "Data", "BeeDayEvents.ndjson");
        return await File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
    }

    private static UserLeveledUpDomainEvent CreateLevelUpEvent(Guid experienceEntryId) =>
        new(
            Guid.NewGuid(),
            experienceEntryId,
            3,
            7,
            4,
            20,
            ExperienceSourceType.Project,
            DateTimeOffset.UtcNow);

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "BeeDay.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
