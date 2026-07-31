using LevelUp.Application.Features.Wallets.Responses;
using LevelUp.Domain.Enums;
using MediatR;

namespace LevelUp.Application.Features.Wallets.Queries;

public sealed record GetWalletSummaryQuery : IRequest<WalletSummaryResponse?>;
public sealed record GetWalletTagsQuery : IRequest<IReadOnlyList<WalletTagResponse>>;
public sealed record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionResponse?>;
public sealed record GetTransactionsQuery(
    string? Search = null,
    TransactionType? Type = null,
    Guid? WalletTagId = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    decimal? MinimumAmount = null,
    decimal? MaximumAmount = null,
    TransactionSortField SortBy = TransactionSortField.Date,
    SortDirection SortDirection = SortDirection.Descending,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedTransactionsResponse>;

public enum TransactionSortField { Date = 1, Description = 2, Amount = 3, CreatedAt = 4 }
public enum SortDirection { Ascending = 1, Descending = 2 }
