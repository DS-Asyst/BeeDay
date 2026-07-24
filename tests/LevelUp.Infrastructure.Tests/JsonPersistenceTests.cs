using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.HealthChecks;
using LevelUp.Infrastructure.Persistence.Exceptions;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LevelUp.Infrastructure.Tests;

public sealed class JsonPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"levelup-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAndLoadAsync_PreservesDomainData()
    {
        var fixture = CreateFixture();
        var data = CreateData("Drink water");
        var cancellationToken = TestContext.Current.CancellationToken;

        await fixture.Repository.SaveAsync(data, cancellationToken);
        var loaded = await fixture.Repository.LoadAsync(cancellationToken);

        Assert.Single(loaded.Habits);
        Assert.Equal("Drink water", loaded.Habits[0].Title);
        Assert.True(File.Exists(fixture.Paths.DataFile));
    }

    [Fact]
    public async Task SaveAsync_CreatesBackupsAndAppliesRetention()
    {
        var fixture = CreateFixture(retention: 2);
        var cancellationToken = TestContext.Current.CancellationToken;

        await fixture.Repository.SaveAsync(CreateData("Version 1"), cancellationToken);
        await fixture.Repository.SaveAsync(CreateData("Version 2"), cancellationToken);
        await fixture.Repository.SaveAsync(CreateData("Version 3"), cancellationToken);
        await fixture.Repository.SaveAsync(CreateData("Version 4"), cancellationToken);

        var backups = Directory.GetFiles(fixture.Paths.BackupDirectory, "*.json");
        Assert.Equal(2, backups.Length);
    }

    [Fact]
    public async Task LoadAsync_RestoresLatestValidBackup_WhenPrimaryFileIsCorrupted()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;

        await fixture.Repository.SaveAsync(CreateData("Recover me"), cancellationToken);
        await fixture.Repository.SaveAsync(CreateData("Current value"), cancellationToken);
        await File.WriteAllTextAsync(fixture.Paths.DataFile, "{ invalid json", cancellationToken);

        var recovered = await fixture.Repository.LoadAsync(cancellationToken);

        Assert.Equal("Recover me", recovered.Habits[0].Title);
        Assert.Contains(
            "Recover me",
            await File.ReadAllTextAsync(fixture.Paths.DataFile, cancellationToken));
    }

    [Fact]
    public async Task LoadAsync_ThrowsBackupRestoreException_WhenPrimaryAndBackupsAreInvalid()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        Directory.CreateDirectory(fixture.Paths.StorageDirectory);
        Directory.CreateDirectory(fixture.Paths.BackupDirectory);
        await File.WriteAllTextAsync(fixture.Paths.DataFile, "invalid", cancellationToken);
        await File.WriteAllTextAsync(
            Path.Combine(fixture.Paths.BackupDirectory, "broken.json"),
            "invalid",
            cancellationToken);

        await Assert.ThrowsAsync<BackupRestoreException>(
            () => fixture.Repository.LoadAsync(cancellationToken));
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy_WhenStorageIsWritableAndValid()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.Repository.SaveAsync(CreateData("Healthy"), cancellationToken);
        var healthCheck = new JsonStorageHealthCheck(fixture.Paths, fixture.Reader);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "json-storage",
                healthCheck,
                failureStatus: null,
                tags: Array.Empty<string>())
        };

        var result = await healthCheck.CheckHealthAsync(context, cancellationToken);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private Fixture CreateFixture(int retention = 10)
    {
        Directory.CreateDirectory(root);
        var options = Options.Create(new JsonStorageOptions
        {
            Directory = "Data",
            FileName = "LevelUpBD.json",
            BackupDirectory = "Backups",
            BackupRetention = retention,
            CreateBackupBeforeSave = true,
            RecoverFromBackup = true,
            WriteIndented = true
        });
        var environment = new TestHostEnvironment(root);
        var paths = new JsonStoragePaths(environment, options);
        var serializerFactory = new JsonSerializerOptionsFactory(options);
        var reader = new JsonFileReader(serializerFactory, NullLogger<JsonFileReader>.Instance);
        var writer = new JsonFileWriter(serializerFactory);
        var backups = new JsonBackupService(paths, options, reader, NullLogger<JsonBackupService>.Instance);
        var repository = new JsonLevelUpRepository(
            paths,
            reader,
            writer,
            backups,
            options,
            NullLogger<JsonLevelUpRepository>.Instance);

        return new Fixture(repository, paths, reader);
    }

    private static LevelUpData CreateData(string title)
    {
        var data = new LevelUpData();
        data.AddUser(User.Create("Persistence Test User", $"persistence-{Guid.NewGuid():N}@levelup.test"));
        data.AddHabit(Habit.Create(
            title,
            null,
            HabitDirection.Both,
            HabitDifficulty.Easy,
            HabitResetCounter.Daily));
        return data;
    }

    private sealed record Fixture(
        JsonLevelUpRepository Repository,
        JsonStoragePaths Paths,
        JsonFileReader Reader);

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LevelUp.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
