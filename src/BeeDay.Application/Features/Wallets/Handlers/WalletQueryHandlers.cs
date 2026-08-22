using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Wallets.Contracts;
using BeeDay.Application.Features.Wallets.Queries;
using BeeDay.Application.Features.Wallets.Responses;
using MediatR;
using FilterSortDirection = BeeDay.Application.Features.Wallets.Contracts.SortDirection;
using FilterSortField = BeeDay.Application.Features.Wallets.Contracts.TransactionSortField;
using QuerySortDirection = BeeDay.Application.Features.Wallets.Queries.SortDirection;
using QuerySortField = BeeDay.Application.Features.Wallets.Queries.TransactionSortField;

namespace BeeDay.Application.Features.Wallets.Handlers;

public sealed class GetWalletSummaryQueryHandler(IWalletReadService walletReadService, ICurrentUserContext currentUser)
    : IRequestHandler<GetWalletSummaryQuery, WalletSummaryResponse?>
{
    public Task<WalletSummaryResponse?> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken) =>
        walletReadService.GetSummaryAsync(CurrentUserGuard.RequireUserId(currentUser), cancellationToken);
}

public sealed class GetWalletTagsQueryHandler(IWalletReadService walletReadService, ICurrentUserContext currentUser)
    : IRequestHandler<GetWalletTagsQuery, IReadOnlyList<WalletTagResponse>>
{
    public Task<IReadOnlyList<WalletTagResponse>> Handle(GetWalletTagsQuery request, CancellationToken cancellationToken) =>
        walletReadService.ListTagsAsync(CurrentUserGuard.RequireUserId(currentUser), cancellationToken);
}

public sealed class GetTransactionByIdQueryHandler(IWalletReadService walletReadService, ICurrentUserContext currentUser)
    : IRequestHandler<GetTransactionByIdQuery, TransactionResponse?>
{
    public Task<TransactionResponse?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken) =>
        walletReadService.GetTransactionAsync(CurrentUserGuard.RequireUserId(currentUser), request.Id, cancellationToken);
}

public sealed class GetTransactionsQueryHandler(IWalletReadService walletReadService, ICurrentUserContext currentUser)
    : IRequestHandler<GetTransactionsQuery, PagedTransactionsResponse>
{
    public Task<PagedTransactionsResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var filter = new TransactionQueryFilter(
            request.Search,
            request.Type,
            request.WalletTagId,
            request.StartDate,
            request.EndDate,
            request.MinimumAmount,
            request.MaximumAmount,
            MapSortField(request.SortBy),
            MapSortDirection(request.SortDirection),
            request.Page,
            request.PageSize);
        return walletReadService.ListTransactionsAsync(CurrentUserGuard.RequireUserId(currentUser), filter, cancellationToken);
    }

    // Explicit mapping, not a cast: QuerySortField/QuerySortDirection (the MediatR request shape) and
    // FilterSortField/FilterSortDirection (the read-service filter shape, in Contracts) are
    // deliberately separate types per docs/architecture/04-dependency-rules.md §3 — they are allowed
    // to diverge, so nothing may assume their underlying values stay aligned.
    private static FilterSortField MapSortField(QuerySortField field) => field switch
    {
        QuerySortField.Date => FilterSortField.Date,
        QuerySortField.Description => FilterSortField.Description,
        QuerySortField.Amount => FilterSortField.Amount,
        QuerySortField.CreatedAt => FilterSortField.CreatedAt,
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unsupported transaction sort field.")
    };

    private static FilterSortDirection MapSortDirection(QuerySortDirection direction) => direction switch
    {
        QuerySortDirection.Ascending => FilterSortDirection.Ascending,
        QuerySortDirection.Descending => FilterSortDirection.Descending,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unsupported sort direction.")
    };
}
