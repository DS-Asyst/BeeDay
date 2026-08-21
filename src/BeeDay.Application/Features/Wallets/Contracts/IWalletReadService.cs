using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Domain.Enums;

namespace BeeDay.Application.Features.Wallets.Contracts;

/// <summary>
/// Read-only projections spanning Wallet, Transaction, and WalletTag for the Wallet screens
/// (summary, tag list with usage counts, filtered/sorted/paginated transaction list). Kept out of
/// <c>ITransactionRepository</c> deliberately — see the rationale on that interface.
/// </summary>
public interface IWalletReadService
{
    public Task<WalletSummaryResponse?> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<WalletTagResponse>> ListTagsAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task<TransactionResponse?> GetTransactionAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);

    public Task<PagedTransactionsResponse> ListTransactionsAsync(
        Guid userId,
        TransactionQueryFilter filter,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-service filter shape — deliberately distinct from
/// <c>Features.Wallets.Queries.GetTransactionsQuery</c> (the MediatR request DTO). The two are
/// allowed to diverge per docs/architecture/04-dependency-rules.md §3 ("Contract DTO ≠ Command ≠
/// Domain Entity..."); today they happen to carry the same fields.
/// </summary>
public sealed record TransactionQueryFilter(
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
    int PageSize = 50);

public enum TransactionSortField { Date = 1, Description = 2, Amount = 3, CreatedAt = 4 }

public enum SortDirection { Ascending = 1, Descending = 2 }
