using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Wallets.Responses;

public sealed record WalletSummaryResponse(
    Guid WalletId,
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpenses,
    int TransactionCount,
    DateTimeOffset UpdatedAtUtc);

public sealed record TransactionResponse(
    Guid Id,
    Guid WalletId,
    string Description,
    decimal Amount,
    decimal SignedAmount,
    TransactionType Type,
    DateOnly TransactionDate,
    Guid? WalletTagId,
    string? WalletTagName,
    string? WalletTagColor,
    string Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record WalletTagResponse(
    Guid Id,
    string Name,
    string Color,
    int TransactionCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record PagedTransactionsResponse(
    IReadOnlyList<TransactionResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
