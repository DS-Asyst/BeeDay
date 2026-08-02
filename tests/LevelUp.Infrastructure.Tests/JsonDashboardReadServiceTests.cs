using LevelUp.Application.Features.Dashboard.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace LevelUp.Infrastructure.Tests;

/// <summary>
/// Validates the real JSON adapter for <see cref="IDashboardReadService"/> — a real temp file on
/// disk through <see cref="JsonLevelUpDocumentStore"/>, not a fake, per
/// docs/testing/01-testing-strategy.md.
/// </summary>
public sealed class JsonDashboardReadServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"levelup-dashboard-read-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task GetAsync_Throws_WhenUserDoesNotExist()
    {
        var fixture = CreateFixture();

        await Assert.ThrowsAsync<InvalidDomainStateException>(
            () => fixture.ReadService.GetAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_ProjectsProfileHabitsTasksProjectsAndWallet()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        User user = null!;
        Habit habit = null!;
        RecurringTask task = null!;
        Project project = null!;
        Todo todo = null!;
        Wallet wallet = null!;
        await fixture.Store.MutateAsync(data =>
        {
            user = User.Create("Dashboard Test User", $"{Guid.NewGuid():N}@levelup.test");
            data.AddUser(user);
            data.CompleteUserProfile(user.Id, "dashboarduser");

            habit = Habit.Create("Drink water", null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily);
            data.AddHabit(user.Id, habit);

            task = RecurringTask.Create("Weekly review", null, TaskRepeat.Weekly);
            data.AddTask(user.Id, task);

            project = Project.Create("Launch", "Ship it");
            data.AddProject(user.Id, project);
            todo = Todo.Create(project.Id, "Write docs", null, null);
            project.AddTodo(todo);

            wallet = Wallet.Create(user.Id);
            data.AddWallet(wallet);
            data.AddTransaction(Transaction.Create(wallet.Id, "Salary", 1000m, TransactionType.Income, new DateOnly(2026, 7, 1)));
        }, cancellationToken);

        var response = await fixture.ReadService.GetAsync(user.Id, cancellationToken);

        Assert.Equal(user.Id, response.Profile.UserId);
        Assert.Equal("dashboarduser", response.Profile.Nickname);
        Assert.True(response.Profile.HasProfile);

        var habitSummary = Assert.Single(response.Habits);
        Assert.Equal(habit.Id, habitSummary.Id);
        Assert.Equal("Drink water", habitSummary.Title);

        var taskSummary = Assert.Single(response.Tasks);
        Assert.Equal(task.Id, taskSummary.Id);

        var projectSummary = Assert.Single(response.Projects);
        Assert.Equal(project.Id, projectSummary.Id);
        var todoSummary = Assert.Single(projectSummary.Todos);
        Assert.Equal(todo.Id, todoSummary.Id);
        Assert.Equal(project.Id, todoSummary.ProjectId);

        Assert.NotNull(response.Wallet);
        Assert.Equal(1000m, response.Wallet.Balance);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWallet_WhenUserHasNoWallet()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        User user = null!;
        await fixture.Store.MutateAsync(data =>
        {
            user = User.Create("No Wallet User", $"{Guid.NewGuid():N}@levelup.test");
            data.AddUser(user);
        }, cancellationToken);

        var response = await fixture.ReadService.GetAsync(user.Id, cancellationToken);

        Assert.Null(response.Wallet);
    }

    [Fact]
    public async Task GetAsync_IsolatesActivitiesBetweenUsers()
    {
        var fixture = CreateFixture();
        var cancellationToken = TestContext.Current.CancellationToken;
        User alice = null!;
        User bob = null!;
        await fixture.Store.MutateAsync(data =>
        {
            alice = User.Create("Alice", $"{Guid.NewGuid():N}@levelup.test");
            data.AddUser(alice);
            bob = User.Create("Bob", $"{Guid.NewGuid():N}@levelup.test");
            data.AddUser(bob);

            data.AddHabit(bob.Id, Habit.Create("Bob's habit", null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily));
            var bobProject = Project.Create("Bob's project", null);
            data.AddProject(bob.Id, bobProject);
        }, cancellationToken);

        var response = await fixture.ReadService.GetAsync(alice.Id, cancellationToken);

        Assert.Empty(response.Habits);
        Assert.Empty(response.Tasks);
        Assert.Empty(response.Projects);
        Assert.Null(response.Wallet);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private Fixture CreateFixture()
    {
        Directory.CreateDirectory(root);
        var options = Options.Create(new JsonStorageOptions
        {
            Directory = "Data",
            FileName = "LevelUpBD.json",
            BackupDirectory = "Backups",
            BackupRetention = 10,
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
        var store = new JsonLevelUpDocumentStore(
            paths,
            reader,
            writer,
            backups,
            new JsonStorageGate(),
            new JsonStorageInitializer(paths),
            new JsonAtomicFileCommitter(),
            options,
            NullLogger<JsonLevelUpDocumentStore>.Instance);
        var readService = new JsonDashboardReadService(store);

        return new Fixture(store, readService);
    }

    private sealed record Fixture(JsonLevelUpDocumentStore Store, JsonDashboardReadService ReadService);

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LevelUp.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
