using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Infrastructure.Persistence.Exceptions;
using BeeDay.Infrastructure.Persistence.SqlServer;
using BeeDay.Infrastructure.Persistence.SqlServer.Repositories;
using BeeDay.Infrastructure.Tests.Persistence.SqlServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BeeDay.Infrastructure.Tests.Persistence.SqlServer;

[Collection("EfLocalDb")]
public sealed class EfUnitOfWorkTests : EfLocalDbTestBase
{
    [Fact]
    public async Task CommitTransactionAsync_PersistsChangesFromMultipleRepositories()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = User.Create("Ada Lovelace", "ada@example.com");
        var habit = CreateHabit(user.Id, "Drink water");

        await using (var unitOfWork = new EfUnitOfWork(ContextFactory))
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Users.AddAsync(user, cancellationToken);
            await unitOfWork.Habits.AddAsync(habit, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        await using var verify = new EfUnitOfWork(ContextFactory);
        Assert.NotNull(await verify.Users.GetByIdAsync(user.Id, cancellationToken));
        Assert.NotNull(await verify.Habits.GetAsync(user.Id, habit.Id, cancellationToken));
    }

    [Fact]
    public async Task RollbackTransactionAsync_DiscardsChangesFromMultipleRepositories()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = User.Create("Grace Hopper", "grace@example.com");
        var habit = CreateHabit(user.Id, "Drink water");

        await using (var unitOfWork = new EfUnitOfWork(ContextFactory))
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Users.AddAsync(user, cancellationToken);
            await unitOfWork.Habits.AddAsync(habit, cancellationToken);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
        }

        await using var verify = new EfUnitOfWork(ContextFactory);
        Assert.Null(await verify.Users.GetByIdAsync(user.Id, cancellationToken));
        Assert.Null(await verify.Habits.GetAsync(user.Id, habit.Id, cancellationToken));
    }

    [Fact]
    public async Task DisposeWithoutCommit_RollsBackAutomatically()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var user = User.Create("Katherine Johnson", "katherine@example.com");

        await using (var unitOfWork = new EfUnitOfWork(ContextFactory))
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Users.AddAsync(user, cancellationToken);
            // Disposed here without ever calling CommitTransactionAsync.
        }

        await using var verify = new EfUnitOfWork(ContextFactory);
        Assert.Null(await verify.Users.GetByIdAsync(user.Id, cancellationToken));
    }

    [Fact]
    public async Task RollbackOnException_DiscardsTheEarlierSuccessfulWriteToo()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await CreateUserIdAsync(cancellationToken);
        var firstWallet = Wallet.Create(userId);
        var secondWallet = Wallet.Create(userId);

        await using var unitOfWork = new EfUnitOfWork(ContextFactory);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.Wallets.AddAsync(firstWallet, cancellationToken);

        await Assert.ThrowsAsync<PersistenceException>(
            () => unitOfWork.Wallets.AddAsync(secondWallet, cancellationToken));

        await unitOfWork.RollbackTransactionAsync(cancellationToken);

        await using var verify = new EfUnitOfWork(ContextFactory);
        Assert.Null(await verify.Wallets.GetByUserAsync(userId, cancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsAMutationOnAnEntityStillTrackedAfterAdd()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await CreateUserIdAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");

        await using (var unitOfWork = new EfUnitOfWork(ContextFactory))
        {
            await unitOfWork.Habits.AddAsync(habit, cancellationToken);

            // `habit` remains tracked in the unit of work's shared context after AddAsync (no
            // detachment happens) — mutating it directly and calling SaveChangesAsync again persists
            // that mutation, without any dedicated Update method.
            habit.SetFeatured(true);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await using var verify = new EfUnitOfWork(ContextFactory);
        var loaded = await verify.Habits.GetAsync(userId, habit.Id, cancellationToken);
        Assert.True(loaded!.Featured);
    }

    [Fact]
    public async Task SaveChangesAsync_ConcurrentModification_ThrowsConcurrencyConflictException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = await CreateUserIdAsync(cancellationToken);
        var habit = CreateHabit(userId, "Drink water");
        await new EfHabitRepository(ContextFactory).AddAsync(habit, cancellationToken);

        await using var contextA = await ContextFactory.CreateDbContextAsync(cancellationToken);
        await using var contextB = await ContextFactory.CreateDbContextAsync(cancellationToken);

        var habitA = await contextA.Habits.SingleAsync(h => h.Id == habit.Id, cancellationToken);
        var habitB = await contextB.Habits.SingleAsync(h => h.Id == habit.Id, cancellationToken);

        // contextB persists first, advancing the row's RowVersion underneath contextA's already-loaded
        // (now stale) copy.
        habitB.SetFeatured(true);
        await contextB.SaveChangesAsync(cancellationToken);

        habitA.SetFeatured(false);
        var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => EfConcurrencySaveChanges.ExecuteAsync(contextA, cancellationToken));

        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    private static Habit CreateHabit(Guid userId, string title)
    {
        var habit = Habit.Create(title, null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(userId);
        return habit;
    }

    private async Task<Guid> CreateUserIdAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
