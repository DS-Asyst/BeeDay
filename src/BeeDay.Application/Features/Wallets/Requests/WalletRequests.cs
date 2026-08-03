using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Wallets.Requests;

public sealed record SaveTransactionRequest(
    string Description,
    decimal Amount,
    TransactionType Type,
    DateOnly TransactionDate,
    Guid? WalletTagId,
    string? Notes);

public sealed record SaveWalletTagRequest(string Name, string? Color);
