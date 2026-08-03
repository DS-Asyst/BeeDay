using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Persistence.Exceptions;
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

    [Fact]
    public async Task UpdateAsync_PersistsTheMutation()
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

        var transaction = Transaction.Create(
            wallet.Id, "Coffee", 5m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow));
        await repository.AddAsync(transaction, cancellationToken);

        await repository.UpdateAsync(wallet.Id, transaction.Id, t => t.AssignTag(tag.Id), cancellationToken);

        var loaded = await repository.GetAsync(wallet.Id, transaction.Id, cancellationToken);
        Assert.Equal(tag.Id, loaded!.WalletTagId);
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentModification_ThrowsConcurrencyConflictException()
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

        var transaction = Transaction.Create(
            wallet.Id, "Coffee", 5m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow));
        await repository.AddAsync(transaction, cancellationToken);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => repository.UpdateAsync(
            wallet.Id,
            transaction.Id,
            t =>
            {
                using var raceContext = ContextFactory.CreateDbContext();
                var raceTransaction = raceContext.Transactions.Single(existing => existing.Id == transaction.Id);
                raceTransaction.AssignTag(tag.Id);
                raceContext.SaveChanges();

                t.RemoveTag();
            },
            cancellationToken));
    }

    [Fact]
    public async Task ClearTagReferencesAsync_ClearsOnlyTransactionsWithThatTag()
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

        var tagged1 = Transaction.Create(
            wallet.Id, "Milk", 5m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), tag.Id);
        var tagged2 = Transaction.Create(
            wallet.Id, "Bread", 3m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), tag.Id);
        var untagged = Transaction.Create(
            wallet.Id, "Salary", 1000m, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow));
        await repository.AddAsync(tagged1, cancellationToken);
        await repository.AddAsync(tagged2, cancellationToken);
        await repository.AddAsync(untagged, cancellationToken);

        await repository.ClearTagReferencesAsync(tag.Id, cancellationToken);

        Assert.Null((await repository.GetAsync(wallet.Id, tagged1.Id, cancellationToken))!.WalletTagId);
        Assert.Null((await repository.GetAsync(wallet.Id, tagged2.Id, cancellationToken))!.WalletTagId);
        Assert.Null((await repository.GetAsync(wallet.Id, untagged.Id, cancellationToken))!.WalletTagId);
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
