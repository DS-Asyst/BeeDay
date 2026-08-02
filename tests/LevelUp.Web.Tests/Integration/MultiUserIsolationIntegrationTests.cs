using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Application.Features.Projects.Commands;
using LevelUp.Application.Features.Projects.Requests;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Application.Features.Tasks.Requests;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Application.Features.Todos.Requests;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Requests;
using LevelUp.Application.Features.Wallets.Commands;
using LevelUp.Application.Features.Wallets.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Web.Tests.Integration;

/// <summary>
/// Proves cross-user isolation for every user-owned aggregate (Habits, Tasks, Projects, Todos,
/// Wallet transactions, Wallet tags, Profile) through the REAL MediatR handlers and the REAL
/// HttpCurrentUserContext, driven by <see cref="LevelUpWebApplicationFactory.CreateAuthenticatedScope"/>.
/// These flows have no raw HTTP endpoint of their own (they're invoked from Blazor components), so
/// this is the closest to end-to-end coverage available without faking ICurrentUserContext itself
/// — a hard constraint for this suite.
/// </summary>
public sealed class MultiUserIsolationIntegrationTests(LevelUpWebApplicationFactory factory)
    : IClassFixture<LevelUpWebApplicationFactory>
{
    [Fact]
    public async Task Snapshot_ForOneUser_NeverIncludesAnotherUsersData()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-snapshot");
        await CreateHabitAsync(bob.Id, cancellationToken);
        await CreateTaskAsync(bob.Id, cancellationToken);
        var projectId = await CreateProjectAsync(bob.Id, cancellationToken);
        await CreateTodoAsync(bob.Id, projectId, cancellationToken);
        await CreateWalletTagAsync(bob.Id, cancellationToken);
        await CreateTransactionAsync(bob.Id, null, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var response = await sender.Send(new GetDashboardQuery(), cancellationToken);

        Assert.Equal(alice.Id, response.Profile.UserId);
        Assert.Empty(response.Habits);
        Assert.Empty(response.Tasks);
        Assert.Empty(response.Projects);
        // Wallet tag/transaction isolation for bob's data is covered separately by
        // JsonWalletReadServiceTests.ListTransactionsAsync_IsolatesTransactionsBetweenUsers
        // (Sprint 13.4 Lot 1) — DashboardResponse doesn't carry raw WalletTags/Transactions lists at
        // all (only a Wallet summary), since the Dashboard page never renders them.
        Assert.Null(response.Wallet);
    }

    [Fact]
    public async Task User_CannotRegisterAnotherUsersHabit()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-habit");
        var habitId = await CreateHabitAsync(bob.Id, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new RegisterHabitPositiveCommand(habitId), cancellationToken));

        var habit = await FindAsync(data => data.Habits.Single(item => item.Id == habitId));
        Assert.Equal(0, habit.PositiveCount);
    }

    [Fact]
    public async Task User_CannotUpdateOrDeleteAnotherUsersTask()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-task");
        var taskId = await CreateTaskAsync(bob.Id, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new UpdateTaskCommand(taskId, new SaveTaskRequest("Hijacked", "", TaskRepeat.None)), cancellationToken));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new DeleteTaskCommand(taskId), cancellationToken));

        var task = await FindAsync(data => data.Tasks.Single(item => item.Id == taskId));
        Assert.Equal("Bob task", task.Title);
    }

    [Fact]
    public async Task User_CannotUpdateOrDeleteAnotherUsersProject()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-project");
        var projectId = await CreateProjectAsync(bob.Id, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new UpdateProjectCommand(projectId, new SaveProjectRequest("Hijacked", "", "#000000", null, false)), cancellationToken));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new DeleteProjectCommand(projectId), cancellationToken));

        var project = await FindAsync(data => data.Projects.Single(item => item.Id == projectId));
        Assert.Equal("Bob project", project.Name);
    }

    [Fact]
    public async Task User_CannotModifyAnotherUsersTodo()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-todo");
        var projectId = await CreateProjectAsync(bob.Id, cancellationToken);
        var todoId = await CreateTodoAsync(bob.Id, projectId, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ToggleTodoCommand(todoId), cancellationToken));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new UpdateTodoCommand(todoId, new SaveTodoRequest(projectId, "Hijacked", "", null)), cancellationToken));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new DeleteTodoCommand(todoId), cancellationToken));

        var project = await FindAsync(data => data.Projects.Single(item => item.Id == projectId));
        var todo = project.Todos.Single(item => item.Id == todoId);
        Assert.False(todo.Completed);
        Assert.Equal("Bob todo", todo.Title);
    }

    [Fact]
    public async Task User_CannotModifyAnotherUsersTransaction()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-transaction");
        var transactionId = await CreateTransactionAsync(bob.Id, null, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(
                new UpdateTransactionCommand(transactionId, new SaveTransactionRequest("Hijacked", 1m, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow), null, null)),
                cancellationToken));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new DeleteTransactionCommand(transactionId), cancellationToken));

        var transaction = await FindAsync(data => data.Transactions.Single(item => item.Id == transactionId));
        Assert.Equal("Bob transaction", transaction.Description);
    }

    [Fact]
    public async Task User_CannotModifyAnotherUsersWalletTag()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-tag");
        var tagId = await CreateWalletTagAsync(bob.Id, cancellationToken);

        using var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new UpdateWalletTagCommand(tagId, new SaveWalletTagRequest("Hijacked", null)), cancellationToken));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new DeleteWalletTagCommand(tagId), cancellationToken));

        var tag = await FindAsync(data => data.WalletTags.Single(item => item.Id == tagId));
        Assert.Equal("Bob tag", tag.Name);
    }

    [Fact]
    public async Task ProfileCommand_OnlyAffectsTheInvokingUsersOwnProfile()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var (alice, bob) = await SeedTwoUsersAsync("isolation-profile");

        using (var scope = factory.CreateAuthenticatedScope(alice.Id, alice.SessionVersion))
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateCurrentUserAvatarCommand(new UpdateUserAvatarRequest("alice-avatar")), cancellationToken);
        }

        var bobAfter = await FindAsync(data => data.FindUser(bob.Id));
        var aliceAfter = await FindAsync(data => data.FindUser(alice.Id));
        Assert.Equal(string.Empty, bobAfter.Avatar);
        Assert.Equal("alice-avatar", aliceAfter.Avatar);
    }

    private async Task<(User Alice, User Bob)> SeedTwoUsersAsync(string prefix)
    {
        var alice = await factory.SeedConfirmedUserAsync($"{prefix}-alice@levelup.invalid", "Password123!");
        var bob = await factory.SeedConfirmedUserAsync($"{prefix}-bob@levelup.invalid", "Password123!");
        return (alice, bob);
    }

    private async Task<Guid> CreateHabitAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateAuthenticatedScope(userId, 1);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new CreateHabitCommand(new SaveHabitRequest("Bob habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily)),
            cancellationToken);
        var habit = await FindAsync(data => data.Habits.Single(item => item.UserId == userId));
        return habit.Id;
    }

    private async Task<Guid> CreateTaskAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateAuthenticatedScope(userId, 1);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new CreateTaskCommand(new SaveTaskRequest("Bob task", "", TaskRepeat.None)),
            cancellationToken);
        var task = await FindAsync(data => data.Tasks.Single(item => item.UserId == userId));
        return task.Id;
    }

    private async Task<Guid> CreateProjectAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateAuthenticatedScope(userId, 1);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new CreateProjectCommand(new SaveProjectRequest("Bob project", "", "#123456", null, false)),
            cancellationToken);
        var project = await FindAsync(data => data.Projects.Single(item => item.UserId == userId));
        return project.Id;
    }

    private async Task<Guid> CreateTodoAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateAuthenticatedScope(userId, 1);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new CreateTodoCommand(new SaveTodoRequest(projectId, "Bob todo", "", null)),
            cancellationToken);
        var project = await FindAsync(data => data.Projects.Single(item => item.Id == projectId));
        return project.Todos.Single(item => item.Title == "Bob todo").Id;
    }

    private async Task<Guid> CreateWalletTagAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateAuthenticatedScope(userId, 1);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new CreateWalletTagCommand(new SaveWalletTagRequest("Bob tag", null)), cancellationToken);
        var tag = await FindAsync(data => data.WalletTags.Single(item => item.UserId == userId));
        return tag.Id;
    }

    private async Task<Guid> CreateTransactionAsync(Guid userId, Guid? tagId, CancellationToken cancellationToken)
    {
        using var scope = factory.CreateAuthenticatedScope(userId, 1);
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(
            new CreateTransactionCommand(new SaveTransactionRequest("Bob transaction", 10m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), tagId, null)),
            cancellationToken);
        var wallet = await FindAsync(data => data.Wallets.Single(item => item.UserId == userId));
        var transaction = await FindAsync(data => data.Transactions.Single(item => item.WalletId == wallet.Id));
        return transaction.Id;
    }

    private async Task<T> FindAsync<T>(Func<LevelUpData, T> select)
    {
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILevelUpRepository>();
        var data = await repository.LoadAsync();
        return select(data);
    }
}
