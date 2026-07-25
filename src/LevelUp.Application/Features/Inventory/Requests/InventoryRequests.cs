using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Inventory.Requests;

public sealed record SaveTransactionRequest(
    string Description,
    decimal Amount,
    TransactionType Type,
    DateOnly TransactionDate,
    Guid? InventoryTagId,
    string? Notes);

public sealed record SaveInventoryTagRequest(string Name, string? Color);
