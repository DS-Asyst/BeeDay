using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Persistence.Exceptions;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfWalletRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetByUserAsync_RoundTripsTheSameWallet()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);

        await repository.AddAsync(wallet, cancellationToken);
        var loaded = await repository.GetByUserAsync(userId, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(wallet.Id, loaded!.Id);
    }

    [Fact]
    public async Task AddAsync_SecondWalletForSameUser_ViolatesUniqueIndex()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        await repository.AddAsync(Wallet.Create(userId), cancellationToken);

        // Proves UX_Wallets_User (Sprint 14.3) is genuinely enforced by SQL Server, not just declared
        // in the EF model. Sprint 14.5 standardizes Infrastructure exceptions (EfConcurrencySaveChanges)
        // — the raw EF Core DbUpdateException no longer leaks past the repository, it comes back as the
        // same PersistenceException hierarchy already used by the JSON provider.
        await Assert.ThrowsAsync<PersistenceException>(
            () => repository.AddAsync(Wallet.Create(userId), cancellationToken));
    }

    [Fact]
    public async Task GetByUserAsync_ForUnknownUser_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletRepository(ContextFactory);

        var loaded = await repository.GetByUserAsync(Guid.NewGuid(), cancellationToken);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task UpdateAsync_PersistsTheMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await repository.AddAsync(wallet, cancellationToken);
        var before = (await repository.GetByUserAsync(userId, cancellationToken))!.UpdatedAtUtc;

        await Task.Delay(10, cancellationToken);
        await repository.UpdateAsync(userId, w => w.Touch(), cancellationToken);

        var loaded = await repository.GetByUserAsync(userId, cancellationToken);
        Assert.True(loaded!.UpdatedAtUtc > before);
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentModification_ThrowsConcurrencyConflictException()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await repository.AddAsync(wallet, cancellationToken);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(
            userId,
            w =>
            {
                using var raceContext = ContextFactory.CreateDbContext();
                var raceWallet = raceContext.Wallets.Single(existing => existing.Id == wallet.Id);
                raceWallet.Touch();
                raceContext.SaveChanges();

                w.Touch();
            },
            cancellationToken));
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
