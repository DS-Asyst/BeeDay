using BeeDay.Application.Features.Wallets.Contracts;
using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer;

/// <summary>
/// SQL Server adapter for <see cref="IWalletReadService"/> — mirrors <c>JsonWalletReadService</c>'s
/// filter/sort/paginate behavior exactly, translated to LINQ-to-Entities. Two deliberate departures from
/// the JSON version's literal code, both behaviorally equivalent: (1) string comparisons use plain
/// <c>Contains</c>/<c>OrderBy</c> instead of an explicit <c>StringComparer.OrdinalIgnoreCase</c> — EF
/// Core cannot translate a custom comparer to SQL, and the database's default case-insensitive collation
/// (already relied on for <c>UX_Users_Email</c>/<c>UX_WalletTags_User_Name</c> uniqueness) provides the
/// same case-insensitive semantics; (2) the WalletTag name/color for each Transaction is looked up via a
/// small follow-up query over the already-paged result set (Domain has no navigation property between
/// Transaction and WalletTag), instead of an in-memory scan of the whole loaded document.
/// </summary>
internal sealed class EfWalletReadService(IDbContextFactory<LevelUpDbContext> contextFactory) : IWalletReadService
{
    public async Task<WalletSummaryResponse?> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var wallet = await context.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.UserId == userId, cancellationToken);
        if (wallet is null)
        {
            return null;
        }

        var transactions = await context.Transactions.AsNoTracking()
            .Where(transaction => transaction.WalletId == wallet.Id)
            .ToListAsync(cancellationToken);

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
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tags = await context.WalletTags.AsNoTracking()
            .Where(tag => tag.UserId == userId)
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);

        var tagIds = tags.Select(tag => tag.Id).ToList();
        var counts = await context.Transactions.AsNoTracking()
            .Where(transaction => transaction.WalletTagId != null && tagIds.Contains(transaction.WalletTagId.Value))
            .GroupBy(transaction => transaction.WalletTagId!.Value)
            .Select(group => new { TagId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.TagId, entry => entry.Count, cancellationToken);

        return tags
            .Select(tag => new WalletTagResponse(
                tag.Id,
                tag.Name,
                tag.Color,
                counts.GetValueOrDefault(tag.Id),
                tag.CreatedAtUtc,
                tag.UpdatedAtUtc))
            .ToList();
    }

    public async Task<TransactionResponse?> GetTransactionAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var wallet = await context.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.UserId == userId, cancellationToken);
        if (wallet is null)
        {
            return null;
        }

        var transaction = await context.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(
                existing => existing.Id == transactionId && existing.WalletId == wallet.Id,
                cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        var tag = transaction.WalletTagId is Guid tagId
            ? await context.WalletTags.AsNoTracking().FirstOrDefaultAsync(existing => existing.Id == tagId, cancellationToken)
            : null;

        return MapTransaction(transaction, tag);
    }

    public async Task<PagedTransactionsResponse> ListTransactionsAsync(
        Guid userId,
        TransactionQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var wallet = await context.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.UserId == userId, cancellationToken);
        if (wallet is null)
        {
            return new([], filter.Page, filter.PageSize, 0, 0);
        }

        var query = context.Transactions.AsNoTracking().Where(transaction => transaction.WalletId == wallet.Id);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(transaction =>
                transaction.Description.Contains(search) || transaction.Notes.Contains(search));
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

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var page = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var tagIds = page.Where(transaction => transaction.WalletTagId is not null)
            .Select(transaction => transaction.WalletTagId!.Value)
            .Distinct()
            .ToList();
        var tagsById = tagIds.Count == 0
            ? new Dictionary<Guid, WalletTag>()
            : await context.WalletTags.AsNoTracking()
                .Where(tag => tagIds.Contains(tag.Id))
                .ToDictionaryAsync(tag => tag.Id, cancellationToken);

        var items = page
            .Select(transaction => MapTransaction(
                transaction,
                transaction.WalletTagId is Guid tagId && tagsById.TryGetValue(tagId, out var tag) ? tag : null))
            .ToList();

        return new(items, filter.Page, filter.PageSize, totalCount, totalPages);
    }

    private static IQueryable<Transaction> ApplyOrdering(
        IQueryable<Transaction> query,
        TransactionSortField field,
        SortDirection direction) => (field, direction) switch
        {
            (TransactionSortField.Description, SortDirection.Ascending) => query.OrderBy(x => x.Description),
            (TransactionSortField.Description, SortDirection.Descending) => query.OrderByDescending(x => x.Description),
            (TransactionSortField.Amount, SortDirection.Ascending) => query.OrderBy(x => x.Amount),
            (TransactionSortField.Amount, SortDirection.Descending) => query.OrderByDescending(x => x.Amount),
            (TransactionSortField.CreatedAt, SortDirection.Ascending) => query.OrderBy(x => x.CreatedAtUtc),
            (TransactionSortField.CreatedAt, SortDirection.Descending) => query.OrderByDescending(x => x.CreatedAtUtc),
            (_, SortDirection.Ascending) => query.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAtUtc)
        };

    private static TransactionResponse MapTransaction(Transaction transaction, WalletTag? tag) => new(
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
