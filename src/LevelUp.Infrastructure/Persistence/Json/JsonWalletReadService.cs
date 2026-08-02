using LevelUp.Application.Features.Wallets.Contracts;
using LevelUp.Application.Features.Wallets.Responses;
using LevelUp.Domain.Entities;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Temporary JSON adapter for <see cref="IWalletReadService"/>. Reproduces the filter/sort/paginate
/// behavior that previously lived in Application's WalletQueryHandlers, now against
/// <see cref="JsonLevelUpDocumentStore"/> instead of <c>ILevelUpRepository</c>. Never exposes
/// <c>LevelUpData</c> — every method returns Application response records only.
/// </summary>
internal sealed class JsonWalletReadService(JsonLevelUpDocumentStore store) : IWalletReadService
{
    public async Task<WalletSummaryResponse?> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var data = await store.LoadAsync(cancellationToken);
        var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == userId);
        if (wallet is null)
        {
            return null;
        }

        var transactions = data.Transactions.Where(transaction => transaction.WalletId == wallet.Id).ToList();
        return new WalletSummaryResponse(
            wallet.Id,
            wallet.CalculateBalance(transactions),
            wallet.CalculateTotalIncome(transactions),
            wallet.CalculateTotalExpenses(transactions),
            transactions.Count,
            wallet.UpdatedAtUtc);
    }

    public async Task<IReadOnlyList<WalletTagResponse>> ListTagsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var data = await store.LoadAsync(cancellationToken);
        return data.WalletTags
            .Where(tag => tag.UserId == userId)
            .OrderBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .Select(tag => new WalletTagResponse(
                tag.Id,
                tag.Name,
                tag.Color,
                data.Transactions.Count(transaction => transaction.WalletTagId == tag.Id),
                tag.CreatedAtUtc,
                tag.UpdatedAtUtc))
            .ToList();
    }

    public async Task<TransactionResponse?> GetTransactionAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var data = await store.LoadAsync(cancellationToken);
        var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == userId);
        if (wallet is null)
        {
            return null;
        }

        var transaction = data.Transactions.FirstOrDefault(candidate => candidate.Id == transactionId && candidate.WalletId == wallet.Id);
        return transaction is null ? null : MapTransaction(data, transaction);
    }

    public async Task<PagedTransactionsResponse> ListTransactionsAsync(
        Guid userId,
        TransactionQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var data = await store.LoadAsync(cancellationToken);
        var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == userId);
        if (wallet is null)
        {
            return new([], filter.Page, filter.PageSize, 0, 0);
        }

        IEnumerable<Transaction> query = data.Transactions.Where(transaction => transaction.WalletId == wallet.Id);
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
            .Select(transaction => MapTransaction(data, transaction)).ToList();
        return new(items, filter.Page, filter.PageSize, totalCount, totalPages);
    }

    private static IEnumerable<Transaction> ApplyOrdering(
        IEnumerable<Transaction> query,
        TransactionSortField field,
        SortDirection direction) => (field, direction) switch
        {
            (TransactionSortField.Description, SortDirection.Ascending) => query.OrderBy(x => x.Description, StringComparer.OrdinalIgnoreCase),
            (TransactionSortField.Description, SortDirection.Descending) => query.OrderByDescending(x => x.Description, StringComparer.OrdinalIgnoreCase),
            (TransactionSortField.Amount, SortDirection.Ascending) => query.OrderBy(x => x.Amount),
            (TransactionSortField.Amount, SortDirection.Descending) => query.OrderByDescending(x => x.Amount),
            (TransactionSortField.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAtUtc),
            (TransactionSortField.CreatedAt, SortDirection.Descending) => query.OrderByDescending(x => x.CreatedAtUtc),
            (_, SortDirection.Ascending) => query.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAtUtc)
        };

    private static TransactionResponse MapTransaction(LevelUpData data, Transaction transaction)
    {
        var tag = transaction.WalletTagId is Guid tagId
            ? data.WalletTags.FirstOrDefault(candidate => candidate.Id == tagId)
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
