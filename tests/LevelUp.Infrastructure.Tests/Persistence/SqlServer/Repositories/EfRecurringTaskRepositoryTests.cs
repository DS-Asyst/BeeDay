using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfRecurringTaskRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetAsync_RoundTripsTheSameTask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfRecurringTaskRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var task = CreateTask(userId, "Water the plants");

        await repository.AddAsync(task, cancellationToken);
        var loaded = await repository.GetAsync(userId, task.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal("Water the plants", loaded!.Title);
    }

    [Fact]
    public async Task ListAsync_ReturnsTasksOrderedByInsertionPosition()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfRecurringTaskRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var first = CreateTask(userId, "First");
        var second = CreateTask(userId, "Second");
        await repository.AddAsync(first, cancellationToken);
        await repository.AddAsync(second, cancellationToken);

        var listed = await repository.ListAsync(userId, cancellationToken);

        Assert.Equal(["First", "Second"], listed.Select(task => task.Title));
    }

    [Fact]
    public async Task ReorderAsync_ChangesTheOrderReturnedByListAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfRecurringTaskRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var first = CreateTask(userId, "First");
        var second = CreateTask(userId, "Second");
        await repository.AddAsync(first, cancellationToken);
        await repository.AddAsync(second, cancellationToken);

        await repository.ReorderAsync(userId, [second.Id, first.Id], cancellationToken);
        var listed = await repository.ListAsync(userId, cancellationToken);

        Assert.Equal(["Second", "First"], listed.Select(task => task.Title));
    }

    [Fact]
    public async Task RemoveAsync_DeletesTheTask()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfRecurringTaskRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var task = CreateTask(userId, "Water the plants");
        await repository.AddAsync(task, cancellationToken);

        await repository.RemoveAsync(task, cancellationToken);
        var loaded = await repository.GetAsync(userId, task.Id, cancellationToken);

        Assert.Null(loaded);
    }

    private static RecurringTask CreateTask(Guid userId, string title)
    {
        var task = RecurringTask.Create(title, null, TaskRepeat.Daily);
        task.AssignOwner(userId);
        return task;
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
