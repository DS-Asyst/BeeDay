using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Persistence.Exceptions;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfHabitRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetAsync_RoundTripsTheSameHabit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");

        await repository.AddAsync(habit, cancellationToken);
        var loaded = await repository.GetAsync(userId, habit.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal("Drink water", loaded!.Title);
    }

    [Fact]
    public async Task GetAsync_ForDifferentUser_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var otherUserId = await CreateUserAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");
        await repository.AddAsync(habit, cancellationToken);

        var loaded = await repository.GetAsync(otherUserId, habit.Id, cancellationToken);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task ListAsync_ReturnsHabitsOrderedByInsertionPosition()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var first = CreateHabit(userId, "First");
        var second = CreateHabit(userId, "Second");
        await repository.AddAsync(first, cancellationToken);
        await repository.AddAsync(second, cancellationToken);

        var listed = await repository.ListAsync(userId, cancellationToken);

        Assert.Equal(["First", "Second"], listed.Select(habit => habit.Title));
    }

    [Fact]
    public async Task ReorderAsync_ChangesTheOrderReturnedByListAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var first = CreateHabit(userId, "First");
        var second = CreateHabit(userId, "Second");
        await repository.AddAsync(first, cancellationToken);
        await repository.AddAsync(second, cancellationToken);

        await repository.ReorderAsync(userId, [second.Id, first.Id], cancellationToken);
        var listed = await repository.ListAsync(userId, cancellationToken);

        Assert.Equal(["Second", "First"], listed.Select(habit => habit.Title));
    }

    [Fact]
    public async Task RemoveAsync_DeletesTheHabit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");
        await repository.AddAsync(habit, cancellationToken);

        await repository.RemoveAsync(habit, cancellationToken);
        var loaded = await repository.GetAsync(userId, habit.Id, cancellationToken);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task UpdateAsync_PersistsTheMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");
        await repository.AddAsync(habit, cancellationToken);

        await repository.UpdateAsync(userId, habit.Id, h => h.SetFeatured(true), cancellationToken);

        var loaded = await repository.GetAsync(userId, habit.Id, cancellationToken);
        Assert.True(loaded!.Featured);
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentModification_ThrowsConcurrencyConflictException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfHabitRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");
        await repository.AddAsync(habit, cancellationToken);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(
            userId,
            habit.Id,
            h =>
            {
                using var raceContext = ContextFactory.CreateDbContext();
                var raceHabit = raceContext.Habits.Single(existing => existing.Id == habit.Id);
                raceHabit.SetFeatured(true);
                raceContext.SaveChanges();

                h.SetFeatured(false);
            },
            cancellationToken));
    }

    private static Habit CreateHabit(Guid userId, string title)
    {
        var habit = Habit.Create(title, null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(userId);
        return habit;
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
