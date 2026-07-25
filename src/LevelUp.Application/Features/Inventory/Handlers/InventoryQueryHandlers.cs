using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Features.Inventory.Queries;
using LevelUp.Application.Features.Inventory.Responses;
using LevelUp.Domain.Entities;
using MediatR;

namespace LevelUp.Application.Features.Inventory.Handlers;

public sealed class GetWalletSummaryQueryHandler(ILevelUpRepository repository)
    : IRequestHandler<GetWalletSummaryQuery, WalletSummaryResponse?>
{
    public async Task<WalletSummaryResponse?> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.LoadAsync(cancellationToken);
        var user = data.CurrentUser;
        if (user is null)
        {
            return null;
        }
        var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == user.Id);
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
}

public sealed class GetInventoryTagsQueryHandler(ILevelUpRepository repository)
    : IRequestHandler<GetInventoryTagsQuery, IReadOnlyList<InventoryTagResponse>>
{
    public async Task<IReadOnlyList<InventoryTagResponse>> Handle(GetInventoryTagsQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.LoadAsync(cancellationToken);
        var user = data.CurrentUser;
        if (user is null)
        {
            return [];
        }
        return data.InventoryTags
            .Where(tag => tag.UserId == user.Id)
            .OrderBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .Select(tag => new InventoryTagResponse(
                tag.Id,
                tag.Name,
                tag.Color,
                data.Transactions.Count(transaction => transaction.InventoryTagId == tag.Id),
                tag.CreatedAtUtc,
                tag.UpdatedAtUtc))
            .ToList();
    }
}

public sealed class GetTransactionByIdQueryHandler(ILevelUpRepository repository)
    : IRequestHandler<GetTransactionByIdQuery, TransactionResponse?>
{
    public async Task<TransactionResponse?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.LoadAsync(cancellationToken);
        var user = data.CurrentUser;
        if (user is null)
        {
            return null;
        }
        var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == user.Id);
        if (wallet is null)
        {
            return null;
        }
        var transaction = data.Transactions.FirstOrDefault(candidate => candidate.Id == request.Id && candidate.WalletId == wallet.Id);
        return transaction is null ? null : InventoryResponseMapper.MapTransaction(data, transaction);
    }
}

public sealed class GetTransactionsQueryHandler(ILevelUpRepository repository)
    : IRequestHandler<GetTransactionsQuery, PagedTransactionsResponse>
{
    public async Task<PagedTransactionsResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.LoadAsync(cancellationToken);
        var user = data.CurrentUser;
        var wallet = user is null ? null : data.Wallets.FirstOrDefault(candidate => candidate.UserId == user.Id);
        if (wallet is null)
        {
            return new([], request.Page, request.PageSize, 0, 0);
        }

        IEnumerable<Transaction> query = data.Transactions.Where(transaction => transaction.WalletId == wallet.Id);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(transaction =>
                transaction.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                transaction.Notes.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
        if (request.Type is not null)
        {
            query = query.Where(transaction => transaction.Type == request.Type);
        }
        if (request.InventoryTagId is not null)
        {
            query = query.Where(transaction => transaction.InventoryTagId == request.InventoryTagId);
        }
        if (request.StartDate is not null)
        {
            query = query.Where(transaction => transaction.TransactionDate >= request.StartDate.Value);
        }
        if (request.EndDate is not null)
        {
            query = query.Where(transaction => transaction.TransactionDate <= request.EndDate.Value);
        }
        if (request.MinimumAmount is not null)
        {
            query = query.Where(transaction => transaction.Amount >= request.MinimumAmount.Value);
        }
        if (request.MaximumAmount is not null)
        {
            query = query.Where(transaction => transaction.Amount <= request.MaximumAmount.Value);
        }

        query = ApplyOrdering(query, request.SortBy, request.SortDirection);
        var totalCount = query.Count();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var items = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(transaction => InventoryResponseMapper.MapTransaction(data, transaction)).ToList();
        return new(items, request.Page, request.PageSize, totalCount, totalPages);
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
}

internal static class InventoryResponseMapper
{
    public static TransactionResponse MapTransaction(LevelUpData data, Transaction transaction)
    {
        var tag = transaction.InventoryTagId is Guid tagId
            ? data.InventoryTags.FirstOrDefault(candidate => candidate.Id == tagId)
            : null;
        return new(
            transaction.Id,
            transaction.WalletId,
            transaction.Description,
            transaction.Amount,
            transaction.SignedAmount,
            transaction.Type,
            transaction.TransactionDate,
            transaction.InventoryTagId,
            tag?.Name,
            tag?.Color,
            transaction.Notes,
            transaction.CreatedAtUtc,
            transaction.UpdatedAtUtc);
    }
}
