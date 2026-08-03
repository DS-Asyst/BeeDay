using BeeDay.Application.Features.Wallets.Handlers;
using BeeDay.Application.Features.Wallets.Queries;
using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using FilterSortDirection = BeeDay.Application.Features.Wallets.Contracts.SortDirection;
using FilterSortField = BeeDay.Application.Features.Wallets.Contracts.TransactionSortField;
using IWalletReadService = BeeDay.Application.Features.Wallets.Contracts.IWalletReadService;
using TransactionQueryFilter = BeeDay.Application.Features.Wallets.Contracts.TransactionQueryFilter;

namespace BeeDay.Application.Tests;

public sealed class WalletHandlersTests
{
    [Fact]
    public async Task EnsureWallet_CreatesSingleWalletForCurrentUser()
    {
        var repo = new FakeUnitOfWork();
        var user = CreateCurrentUser(repo);
        var handler = new EnsureCurrentWalletCommandHandler(repo.Wallets, new FakeCurrentUserContext(user.Id));
        var first = await handler.Handle(new(), TestContext.Current.CancellationToken);
        var second = await handler.Handle(new(), TestContext.Current.CancellationToken);
        Assert.Equal(first, second);
        Assert.Single(repo.WalletsData);
    }

    [Fact]
    public async Task CreateTransaction_CreatesWalletAndUpdatesSummary()
    {
        var repo = new FakeUnitOfWork();
        var user = CreateCurrentUser(repo);
        var context = new FakeCurrentUserContext(user.Id);
        var create = new CreateTransactionCommandHandler(repo, context);
        await create.Handle(new(new("Salary", 1500m, TransactionType.Income, new DateOnly(2026, 7, 25), null, null)), TestContext.Current.CancellationToken);
        await create.Handle(new(new("Internet", 80m, TransactionType.Expense, new DateOnly(2026, 7, 25), null, null)), TestContext.Current.CancellationToken);
        var summary = await new GetWalletSummaryQueryHandler(new FakeWalletReadService(repo), context).Handle(new(), TestContext.Current.CancellationToken);
        Assert.NotNull(summary);
        Assert.Equal(1420m, summary.Balance);
        Assert.Equal(1500m, summary.TotalIncome);
        Assert.Equal(80m, summary.TotalExpenses);
        Assert.Equal(2, summary.TransactionCount);
    }

    [Fact]
    public async Task CreateTransaction_RejectsTagFromAnotherUser()
    {
        var repo = new FakeUnitOfWork();
        var current = CreateCurrentUser(repo);
        var other = User.Create("Other", "other@levelup.invalid");
        repo.UsersData.Add(other);
        var tag = WalletTag.Create(other.Id, "Private");
        repo.WalletTagsData.Add(tag);
        var handler = new CreateTransactionCommandHandler(repo, new FakeCurrentUserContext(current.Id));
        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new(new("Invalid", 10m, TransactionType.Expense, new DateOnly(2026, 7, 25), tag.Id, null)),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteTag_RemovesAssociationAndKeepsTransaction()
    {
        var repo = new FakeUnitOfWork();
        var user = CreateCurrentUser(repo);
        var context = new FakeCurrentUserContext(user.Id);
        var tagId = await new CreateWalletTagCommandHandler(repo.WalletTags, context).Handle(new(new("Food", "#123456")), TestContext.Current.CancellationToken);
        var transactionId = await new CreateTransactionCommandHandler(repo, context).Handle(
            new(new("Lunch", 20m, TransactionType.Expense, new DateOnly(2026, 7, 25), tagId, null)),
            TestContext.Current.CancellationToken);
        await new DeleteWalletTagCommandHandler(repo, context).Handle(new(tagId), TestContext.Current.CancellationToken);
        Assert.Empty(repo.WalletTagsData);
        Assert.Null(repo.TransactionsData.Single(t => t.Id == transactionId).WalletTagId);
    }

    [Fact]
    public async Task GetTransactions_FiltersSortsAndPaginates()
    {
        var repo = new FakeUnitOfWork();
        var user = CreateCurrentUser(repo);
        var context = new FakeCurrentUserContext(user.Id);
        var create = new CreateTransactionCommandHandler(repo, context);
        await create.Handle(new(new("Salary", 1000m, TransactionType.Income, new DateOnly(2026, 7, 1), null, "Monthly")), TestContext.Current.CancellationToken);
        await create.Handle(new(new("Groceries", 200m, TransactionType.Expense, new DateOnly(2026, 7, 2), null, "Food")), TestContext.Current.CancellationToken);
        await create.Handle(new(new("Coffee", 10m, TransactionType.Expense, new DateOnly(2026, 7, 3), null, "Food")), TestContext.Current.CancellationToken);
        var result = await new GetTransactionsQueryHandler(new FakeWalletReadService(repo), context).Handle(
            new(Search: "Food", Type: TransactionType.Expense, SortBy: TransactionSortField.Amount, SortDirection: SortDirection.Descending, Page: 1, PageSize: 1),
            TestContext.Current.CancellationToken);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("Groceries", Assert.Single(result.Items).Description);
    }

    [Fact]
    public async Task CreateTag_RejectsDuplicateNameForCurrentUser()
    {
        var repo = new FakeUnitOfWork();
        var user = CreateCurrentUser(repo);
        var context = new FakeCurrentUserContext(user.Id);
        var handler = new CreateWalletTagCommandHandler(repo.WalletTags, context);
        await handler.Handle(new(new("Food", null)), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new(new("Food", null)), TestContext.Current.CancellationToken));
        Assert.Single(repo.WalletTagsData);
    }

    [Fact]
    public async Task UpdateTag_RejectsDuplicateNameForCurrentUser()
    {
        var repo = new FakeUnitOfWork();
        var user = CreateCurrentUser(repo);
        var context = new FakeCurrentUserContext(user.Id);
        var first = await new CreateWalletTagCommandHandler(repo.WalletTags, context).Handle(new(new("Food", null)), TestContext.Current.CancellationToken);
        var second = await new CreateWalletTagCommandHandler(repo.WalletTags, context).Handle(new(new("Transport", null)), TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<InvalidDomainStateException>(() => new UpdateWalletTagCommandHandler(repo.WalletTags, context).Handle(
            new(second, new(" food ", null)), TestContext.Current.CancellationToken));
        Assert.NotEqual(first, second);
    }

    private static User CreateCurrentUser(FakeUnitOfWork repo)
    {
        var user = User.Create("Test User", $"{Guid.NewGuid():N}@levelup.invalid");
        repo.UsersData.Add(user);
        return user;
    }

    /// <summary>
    /// Minimal in-memory double for <see cref="IWalletReadService"/> — mirrors
    /// BeeDay.Infrastructure's <c>EfWalletReadService</c> filter/sort/paginate behavior against the
    /// per-Aggregate in-memory lists on <see cref="FakeUnitOfWork"/>, so Application tests never need
    /// to reference Infrastructure.
    /// </summary>
    private sealed class FakeWalletReadService(FakeUnitOfWork data) : IWalletReadService
    {
        public Task<WalletSummaryResponse?> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var wallet = data.WalletsData.FirstOrDefault(candidate => candidate.UserId == userId);
            if (wallet is null)
            {
                return Task.FromResult<WalletSummaryResponse?>(null);
            }

            var transactions = data.TransactionsData.Where(transaction => transaction.WalletId == wallet.Id).ToList();
            return Task.FromResult<WalletSummaryResponse?>(new WalletSummaryResponse(
                wallet.Id,
                wallet.CalculateBalance(transactions),
                wallet.CalculateTotalIncome(transactions),
                wallet.CalculateTotalExpenses(transactions),
                transactions.Count,
                wallet.UpdatedAtUtc));
        }

        public Task<IReadOnlyList<WalletTagResponse>> ListTagsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<WalletTagResponse> tags = data.WalletTagsData
                .Where(tag => tag.UserId == userId)
                .OrderBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
                .Select(tag => new WalletTagResponse(
                    tag.Id,
                    tag.Name,
                    tag.Color,
                    data.TransactionsData.Count(transaction => transaction.WalletTagId == tag.Id),
                    tag.CreatedAtUtc,
                    tag.UpdatedAtUtc))
                .ToList();
            return Task.FromResult(tags);
        }

        public Task<TransactionResponse?> GetTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
        {
            var wallet = data.WalletsData.FirstOrDefault(candidate => candidate.UserId == userId);
            if (wallet is null)
            {
                return Task.FromResult<TransactionResponse?>(null);
            }

            var transaction = data.TransactionsData.FirstOrDefault(candidate => candidate.Id == transactionId && candidate.WalletId == wallet.Id);
            return Task.FromResult(transaction is null ? null : Map(transaction));
        }

        public Task<PagedTransactionsResponse> ListTransactionsAsync(Guid userId, TransactionQueryFilter filter, CancellationToken cancellationToken = default)
        {
            var wallet = data.WalletsData.FirstOrDefault(candidate => candidate.UserId == userId);
            if (wallet is null)
            {
                return Task.FromResult(new PagedTransactionsResponse([], filter.Page, filter.PageSize, 0, 0));
            }

            IEnumerable<Transaction> query = data.TransactionsData.Where(transaction => transaction.WalletId == wallet.Id);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(transaction =>
                    transaction.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    transaction.Notes.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.Type is not null)
            {
                query = query.Where(transaction => transaction.Type == filter.Type);
            }
            if (filter.WalletTagId is not null)
            {
                query = query.Where(transaction => transaction.WalletTagId == filter.WalletTagId);
            }
            if (filter.StartDate is not null)
            {
                query = query.Where(transaction => transaction.TransactionDate >= filter.StartDate.Value);
            }
            if (filter.EndDate is not null)
            {
                query = query.Where(transaction => transaction.TransactionDate <= filter.EndDate.Value);
            }
            if (filter.MinimumAmount is not null)
            {
                query = query.Where(transaction => transaction.Amount >= filter.MinimumAmount.Value);
            }
            if (filter.MaximumAmount is not null)
            {
                query = query.Where(transaction => transaction.Amount <= filter.MaximumAmount.Value);
            }

            query = ApplyOrdering(query, filter.SortBy, filter.SortDirection);
            var totalCount = query.Count();
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            var items = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(Map).ToList();
            return Task.FromResult(new PagedTransactionsResponse(items, filter.Page, filter.PageSize, totalCount, totalPages));
        }

        private static IEnumerable<Transaction> ApplyOrdering(
            IEnumerable<Transaction> query,
            FilterSortField field,
            FilterSortDirection direction) => (field, direction) switch
            {
                (FilterSortField.Description, FilterSortDirection.Ascending) => query.OrderBy(x => x.Description, StringComparer.OrdinalIgnoreCase),
                (FilterSortField.Description, FilterSortDirection.Descending) => query.OrderByDescending(x => x.Description, StringComparer.OrdinalIgnoreCase),
                (FilterSortField.Amount, FilterSortDirection.Ascending) => query.OrderBy(x => x.Amount),
                (FilterSortField.Amount, FilterSortDirection.Descending) => query.OrderByDescending(x => x.Amount),
                (FilterSortField.CreatedAt, FilterSortDirection.Ascending) => query.OrderBy(x => x.CreatedAtUtc),
                (FilterSortField.CreatedAt, FilterSortDirection.Descending) => query.OrderByDescending(x => x.CreatedAtUtc),
                (_, FilterSortDirection.Ascending) => query.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreatedAtUtc),
                _ => query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAtUtc)
            };

        private TransactionResponse Map(Transaction transaction)
        {
            var tag = transaction.WalletTagId is Guid tagId
                ? data.WalletTagsData.FirstOrDefault(candidate => candidate.Id == tagId)
                : null;
            return new(
                transaction.Id,
                transaction.WalletId,
                transaction.Description,
                transaction.Amount,
                transaction.SignedAmount,
                transaction.Type,
                transaction.TransactionDate,
                transaction.WalletTagId,
                tag?.Name,
                tag?.Color,
                transaction.Notes,
                transaction.CreatedAtUtc,
                transaction.UpdatedAtUtc);
        }
    }
}
