using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Inventory.Responses;

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
    Guid? InventoryTagId,
    string? InventoryTagName,
    string? InventoryTagColor,
    string Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record InventoryTagResponse(
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
