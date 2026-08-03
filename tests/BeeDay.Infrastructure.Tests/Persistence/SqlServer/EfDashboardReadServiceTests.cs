using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Infrastructure.Persistence.SqlServer;
using BeeDay.Infrastructure.Persistence.SqlServer.Repositories;
using BeeDay.Infrastructure.Tests.Persistence.SqlServer.Repositories;
using Xunit;

namespace BeeDay.Infrastructure.Tests.Persistence.SqlServer;

[Collection("EfLocalDb")]
public sealed class EfDashboardReadServiceTests : EfLocalDbTestBase
{
    [Fact]
    public async Task GetAsync_ReturnsProfileHabitsTasksProjectsAndWallet()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfDashboardReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);

        var habit = Habit.Create("Drink water", null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(userId);
        await new EfHabitRepository(ContextFactory).AddAsync(habit, cancellationToken);

        var task = RecurringTask.Create("Water plants", null, TaskRepeat.Daily);
        task.AssignOwner(userId);
        await new EfRecurringTaskRepository(ContextFactory).AddAsync(task, cancellationToken);

        var project = Project.Create("Launch", null);
        project.AssignOwner(userId);
        var todo = Todo.Create(Guid.NewGuid(), "First todo", null, null);
        todo.AssignOwner(userId);
        project.AddTodo(todo);
        await new EfProjectRepository(ContextFactory).AddAsync(project, cancellationToken);

        var wallet = Wallet.Create(userId);
        await new EfWalletRepository(ContextFactory).AddAsync(wallet, cancellationToken);
        var transaction = Transaction.Create(
            wallet.Id, "Salary", 1000m, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow));
        await new EfTransactionRepository(ContextFactory).AddAsync(transaction, cancellationToken);

        var response = await service.GetAsync(userId, cancellationToken);

        Assert.Equal(userId, response.Profile.UserId);
        Assert.Single(response.Habits);
        Assert.Equal("Drink water", response.Habits[0].Title);
        Assert.Single(response.Tasks);
        Assert.Equal("Water plants", response.Tasks[0].Title);
        Assert.Single(response.Projects);
        Assert.Single(response.Projects[0].Todos);
        Assert.Equal("First todo", response.Projects[0].Todos[0].Title);
        Assert.NotNull(response.Wallet);
        Assert.Equal(1000m, response.Wallet!.Balance);
        Assert.Equal(1, response.Wallet.TransactionCount);
    }

    [Fact]
    public async Task GetAsync_WithNoWallet_ReturnsNullWalletSummary()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfDashboardReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);

        var response = await service.GetAsync(userId, cancellationToken);

        Assert.Null(response.Wallet);
        Assert.Empty(response.Habits);
        Assert.Empty(response.Tasks);
        Assert.Empty(response.Projects);
    }

    [Fact]
    public async Task GetAsync_ForUnknownUser_Throws()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfDashboardReadService(ContextFactory);

        await Assert.ThrowsAsync<InvalidDomainStateException>(
            () => service.GetAsync(Guid.NewGuid(), cancellationToken));
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
