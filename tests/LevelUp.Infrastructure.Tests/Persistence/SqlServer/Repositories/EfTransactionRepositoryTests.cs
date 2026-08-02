using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfTransactionRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetAsync_RoundTripsTheSameTransaction()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var walletRepository = new EfWalletRepository(ContextFactory);
        var repository = new EfTransactionRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await walletRepository.AddAsync(wallet, cancellationToken);

        var transaction = Transaction.Create(
            wallet.Id, "Groceries", 42.50m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow));

        await repository.AddAsync(transaction, cancellationToken);
        var loaded = await repository.GetAsync(wallet.Id, transaction.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(42.50m, loaded!.Amount);
    }

    [Fact]
    public async Task ListByTagAsync_FiltersByWalletTagId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var walletRepository = new EfWalletRepository(ContextFactory);
        var tagRepository = new EfWalletTagRepository(ContextFactory);
        var repository = new EfTransactionRepository(ContextFactory);

        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await walletRepository.AddAsync(wallet, cancellationToken);
        var tag = WalletTag.Create(userId, "Groceries");
        await tagRepository.AddAsync(tag, cancellationToken);

        var tagged = Transaction.Create(
            wallet.Id, "Milk", 5m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), tag.Id);
        var untagged = Transaction.Create(
            wallet.Id, "Salary", 1000m, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow));

        await repository.AddAsync(tagged, cancellationToken);
        await repository.AddAsync(untagged, cancellationToken);

        var taggedOnly = await repository.ListByTagAsync(tag.Id, cancellationToken);

        Assert.Single(taggedOnly);
        Assert.Equal(tagged.Id, taggedOnly[0].Id);
    }

    [Fact]
    public async Task RemoveAsync_DeletesTheTransaction()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var walletRepository = new EfWalletRepository(ContextFactory);
        var repository = new EfTransactionRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await walletRepository.AddAsync(wallet, cancellationToken);

        var transaction = Transaction.Create(
            wallet.Id, "Groceries", 42.50m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow));
        await repository.AddAsync(transaction, cancellationToken);

        await repository.RemoveAsync(transaction, cancellationToken);
        var loaded = await repository.GetAsync(wallet.Id, transaction.Id, cancellationToken);

        Assert.Null(loaded);
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
