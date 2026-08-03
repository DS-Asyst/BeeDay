using LevelUp.Application.Features.Wallets.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Persistence.SqlServer;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer;

[Collection("EfLocalDb")]
public sealed class EfWalletReadServiceTests : EfLocalDbTestBase
{
    [Fact]
    public async Task GetSummaryAsync_WithNoWallet_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfWalletReadService(ContextFactory);

        var summary = await service.GetSummaryAsync(Guid.NewGuid(), cancellationToken);

        Assert.Null(summary);
    }

    [Fact]
    public async Task GetSummaryAsync_CalculatesBalanceIncomeAndExpenses()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfWalletReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await new EfWalletRepository(ContextFactory).AddAsync(wallet, cancellationToken);
        var transactionRepository = new EfTransactionRepository(ContextFactory);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Salary", 1000m, TransactionType.Income, DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Rent", 300m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);

        var summary = await service.GetSummaryAsync(userId, cancellationToken);

        Assert.NotNull(summary);
        Assert.Equal(700m, summary!.Balance);
        Assert.Equal(1000m, summary.TotalIncome);
        Assert.Equal(300m, summary.TotalExpenses);
        Assert.Equal(2, summary.TransactionCount);
    }

    [Fact]
    public async Task ListTagsAsync_ReturnsTagsOrderedByNameWithTransactionCounts()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfWalletReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await new EfWalletRepository(ContextFactory).AddAsync(wallet, cancellationToken);
        var tagRepository = new EfWalletTagRepository(ContextFactory);
        var groceries = WalletTag.Create(userId, "Groceries");
        var bills = WalletTag.Create(userId, "Bills");
        await tagRepository.AddAsync(groceries, cancellationToken);
        await tagRepository.AddAsync(bills, cancellationToken);

        var transactionRepository = new EfTransactionRepository(ContextFactory);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Milk", 5m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), groceries.Id),
            cancellationToken);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Bread", 3m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), groceries.Id),
            cancellationToken);

        var tags = await service.ListTagsAsync(userId, cancellationToken);

        Assert.Equal(["Bills", "Groceries"], tags.Select(tag => tag.Name));
        Assert.Equal(2, tags.Single(tag => tag.Name == "Groceries").TransactionCount);
        Assert.Equal(0, tags.Single(tag => tag.Name == "Bills").TransactionCount);
    }

    [Fact]
    public async Task GetTransactionAsync_IncludesTagNameAndColor()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfWalletReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await new EfWalletRepository(ContextFactory).AddAsync(wallet, cancellationToken);
        var tag = WalletTag.Create(userId, "Groceries");
        await new EfWalletTagRepository(ContextFactory).AddAsync(tag, cancellationToken);
        var transaction = Transaction.Create(
            wallet.Id, "Milk", 5m, TransactionType.Expense, DateOnly.FromDateTime(DateTime.UtcNow), tag.Id);
        await new EfTransactionRepository(ContextFactory).AddAsync(transaction, cancellationToken);

        var response = await service.GetTransactionAsync(userId, transaction.Id, cancellationToken);

        Assert.NotNull(response);
        Assert.Equal("Groceries", response!.WalletTagName);
        Assert.Equal(tag.Color, response.WalletTagColor);
    }

    [Fact]
    public async Task GetTransactionAsync_ForUnknownTransaction_ReturnsNull()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfWalletReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await new EfWalletRepository(ContextFactory).AddAsync(wallet, cancellationToken);

        var response = await service.GetTransactionAsync(userId, Guid.NewGuid(), cancellationToken);

        Assert.Null(response);
    }

    [Fact]
    public async Task ListTransactionsAsync_FiltersSortsAndPaginates()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = new EfWalletReadService(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var wallet = Wallet.Create(userId);
        await new EfWalletRepository(ContextFactory).AddAsync(wallet, cancellationToken);
        var transactionRepository = new EfTransactionRepository(ContextFactory);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Coffee", 5m, TransactionType.Expense, today), cancellationToken);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Salary", 1000m, TransactionType.Income, today), cancellationToken);
        await transactionRepository.AddAsync(
            Transaction.Create(wallet.Id, "Coffee beans", 15m, TransactionType.Expense, today), cancellationToken);

        var expensesOnly = await service.ListTransactionsAsync(
            userId,
            new TransactionQueryFilter(Type: TransactionType.Expense, SortBy: TransactionSortField.Amount, SortDirection: SortDirection.Ascending),
            cancellationToken);

        Assert.Equal(2, expensesOnly.TotalCount);
        Assert.Equal(["Coffee", "Coffee beans"], expensesOnly.Items.Select(item => item.Description));

        var searchResult = await service.ListTransactionsAsync(
            userId, new TransactionQueryFilter(Search: "beans"), cancellationToken);
        Assert.Single(searchResult.Items);
        Assert.Equal("Coffee beans", searchResult.Items[0].Description);

        var pagedResult = await service.ListTransactionsAsync(
            userId, new TransactionQueryFilter(Page: 1, PageSize: 2), cancellationToken);
        Assert.Equal(3, pagedResult.TotalCount);
        Assert.Equal(2, pagedResult.TotalPages);
        Assert.Equal(2, pagedResult.Items.Count);
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
