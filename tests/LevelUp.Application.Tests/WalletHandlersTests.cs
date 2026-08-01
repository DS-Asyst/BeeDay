using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Wallets.Handlers;
using LevelUp.Application.Features.Wallets.Queries;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class WalletHandlersTests
{
    [Fact]
    public async Task EnsureWallet_CreatesSingleWalletForCurrentUser()
    {
        var repo = new Repo();
        var user = repo.CreateCurrentUser();
        var handler = new EnsureCurrentWalletCommandHandler(repo, new UserContext(user.Id));
        var first = await handler.Handle(new(), TestContext.Current.CancellationToken);
        var second = await handler.Handle(new(), TestContext.Current.CancellationToken);
        Assert.Equal(first, second);
        Assert.Single(repo.Data.Wallets);
    }

    [Fact]
    public async Task CreateTransaction_CreatesWalletAndUpdatesSummary()
    {
        var repo = new Repo();
        var user = repo.CreateCurrentUser();
        var context = new UserContext(user.Id);
        var create = new CreateTransactionCommandHandler(repo, context);
        await create.Handle(new(new("Salary", 1500m, TransactionType.Income, new DateOnly(2026, 7, 25), null, null)), TestContext.Current.CancellationToken);
        await create.Handle(new(new("Internet", 80m, TransactionType.Expense, new DateOnly(2026, 7, 25), null, null)), TestContext.Current.CancellationToken);
        var summary = await new GetWalletSummaryQueryHandler(repo, context).Handle(new(), TestContext.Current.CancellationToken);
        Assert.NotNull(summary);
        Assert.Equal(1420m, summary.Balance);
        Assert.Equal(1500m, summary.TotalIncome);
        Assert.Equal(80m, summary.TotalExpenses);
        Assert.Equal(2, summary.TransactionCount);
    }

    [Fact]
    public async Task CreateTransaction_RejectsTagFromAnotherUser()
    {
        var repo = new Repo();
        var current = repo.CreateCurrentUser();
        var other = User.Create("Other", "other@levelup.invalid");
        repo.Data.AddUser(other);
        var tag = WalletTag.Create(other.Id, "Private");
        repo.Data.AddWalletTag(tag);
        var handler = new CreateTransactionCommandHandler(repo, new UserContext(current.Id));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new(new("Invalid", 10m, TransactionType.Expense, new DateOnly(2026, 7, 25), tag.Id, null)),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteTag_RemovesAssociationAndKeepsTransaction()
    {
        var repo = new Repo();
        var user = repo.CreateCurrentUser();
        var context = new UserContext(user.Id);
        var tagId = await new CreateWalletTagCommandHandler(repo, context).Handle(new(new("Food", "#123456")), TestContext.Current.CancellationToken);
        var transactionId = await new CreateTransactionCommandHandler(repo, context).Handle(
            new(new("Lunch", 20m, TransactionType.Expense, new DateOnly(2026, 7, 25), tagId, null)),
            TestContext.Current.CancellationToken);
        await new DeleteWalletTagCommandHandler(repo, context).Handle(new(tagId), TestContext.Current.CancellationToken);
        Assert.Empty(repo.Data.WalletTags);
        Assert.Null(repo.Data.FindTransaction(transactionId).WalletTagId);
    }

    [Fact]
    public async Task GetTransactions_FiltersSortsAndPaginates()
    {
        var repo = new Repo();
        var user = repo.CreateCurrentUser();
        var context = new UserContext(user.Id);
        var create = new CreateTransactionCommandHandler(repo, context);
        await create.Handle(new(new("Salary", 1000m, TransactionType.Income, new DateOnly(2026, 7, 1), null, "Monthly")), TestContext.Current.CancellationToken);
        await create.Handle(new(new("Groceries", 200m, TransactionType.Expense, new DateOnly(2026, 7, 2), null, "Food")), TestContext.Current.CancellationToken);
        await create.Handle(new(new("Coffee", 10m, TransactionType.Expense, new DateOnly(2026, 7, 3), null, "Food")), TestContext.Current.CancellationToken);
        var result = await new GetTransactionsQueryHandler(repo, context).Handle(
            new(Search: "Food", Type: TransactionType.Expense, SortBy: TransactionSortField.Amount, SortDirection: SortDirection.Descending, Page: 1, PageSize: 1),
            TestContext.Current.CancellationToken);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("Groceries", Assert.Single(result.Items).Description);
    }

    [Fact]
    public async Task UpdateTag_RejectsDuplicateNameForCurrentUser()
    {
        var repo = new Repo();
        var user = repo.CreateCurrentUser();
        var context = new UserContext(user.Id);
        var first = await new CreateWalletTagCommandHandler(repo, context).Handle(new(new("Food", null)), TestContext.Current.CancellationToken);
        var second = await new CreateWalletTagCommandHandler(repo, context).Handle(new(new("Transport", null)), TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<InvalidDomainStateException>(() => new UpdateWalletTagCommandHandler(repo, context).Handle(
            new(second, new(" food ", null)), TestContext.Current.CancellationToken));
        Assert.NotEqual(first, second);
    }

    private sealed record UserContext(Guid Id) : ICurrentUserContext
    {
        public Guid? UserId => Id;
    }

    private sealed class Repo : LevelUp.Application.Common.Contracts.ILevelUpRepository
    {
        public LevelUpData Data { get; } = new();
        public User CreateCurrentUser()
        {
            var user = User.Create("Test User", $"{Guid.NewGuid():N}@levelup.invalid");
            Data.AddUser(user);
            return user;
        }
        public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Data);
        public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
        {
            mutation(Data);
            return Task.CompletedTask;
        }
    }
}
