using System.Text.Json;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Events;
using LevelUp.Infrastructure.Auditing;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace LevelUp.Infrastructure.Tests;

public sealed class JsonEventJournalTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"levelup-journal-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task Repeated_event_id_is_written_only_once()
    {
        JsonEventJournal journal = CreateJournal();
        CharacterLeveledUpDomainEvent domainEvent = CreateLevelUpEvent(Guid.NewGuid());

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
        CharacterLeveledUpDomainEvent domainEvent = CreateLevelUpEvent(Guid.NewGuid());

        await journal.AppendAsync(domainEvent, TestContext.Current.CancellationToken);

        string line = Assert.Single(await ReadJournalLinesAsync());
        using JsonDocument document = JsonDocument.Parse(line);
        JsonElement root = document.RootElement;
        JsonElement payload = root.GetProperty("payload");

        Assert.Equal(nameof(CharacterLeveledUpDomainEvent), root.GetProperty("type").GetString());
        Assert.Equal(
            "Character reached level 7 after gaining 20 XP from Project.",
            root.GetProperty("summary").GetString());
        Assert.Equal(domainEvent.CharacterId, payload.GetProperty("characterId").GetGuid());
        Assert.Equal(domainEvent.ExperienceEntryId, payload.GetProperty("experienceEntryId").GetGuid());
        Assert.Equal(3, payload.GetProperty("previousLevel").GetInt32());
        Assert.Equal(7, payload.GetProperty("newLevel").GetInt32());
        Assert.Equal(4, payload.GetProperty("levelsGained").GetInt32());
        Assert.Equal("Project", payload.GetProperty("experienceSource").GetString());
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
        IOptions<JsonStorageOptions> options = Options.Create(new JsonStorageOptions
        {
            Directory = "Data",
            FileName = "LevelUpBD.json",
            WriteIndented = false,
        });
        var paths = new JsonStoragePaths(environment, options);
        var serializerOptionsFactory = new JsonSerializerOptionsFactory(options);
        return new JsonEventJournal(paths, serializerOptionsFactory);
    }

    private async Task<string[]> ReadJournalLinesAsync()
    {
        string path = Path.Combine(_root, "Data", "LevelUpEvents.ndjson");
        return await File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
    }

    private static CharacterLeveledUpDomainEvent CreateLevelUpEvent(Guid experienceEntryId) =>
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
        public string ApplicationName { get; set; } = "LevelUp.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
