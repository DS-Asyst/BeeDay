using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class Transaction : Entity
{
    public const int MaximumDescriptionLength = 120;
    public const int MaximumNotesLength = 500;

    public Guid WalletId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public DateOnly TransactionDate { get; private set; }

    public Guid? WalletTagId { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public decimal SignedAmount => Type == TransactionType.Income ? Amount : -Amount;

    public static Transaction Create(
        Guid walletId,
        string description,
        decimal amount,
        TransactionType type,
        DateOnly transactionDate,
        Guid? walletTagId = null,
        string? notes = null)
    {
        if (walletId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(walletId), "Wallet identifier is required.");
        }

        var transaction = new Transaction { WalletId = walletId };
        transaction.Update(description, amount, type, transactionDate, walletTagId, notes);
        return transaction;
    }

    public void Update(
        string description,
        decimal amount,
        TransactionType type,
        DateOnly transactionDate,
        Guid? walletTagId,
        string? notes)
    {
        Description = ValidateDescription(description);
        Amount = ValidateAmount(amount);
        Type = EnumValidation.Defined(type, nameof(type));
        if (transactionDate == default)
        {
            throw new DomainValidationException(nameof(transactionDate), "Transaction date is required.");
        }
        TransactionDate = transactionDate;
        WalletTagId = ValidateTagId(walletTagId);
        Notes = ValidateNotes(notes);
        Touch();
    }

    public void AssignTag(Guid walletTagId)
    {
        WalletTagId = ValidateTagId(walletTagId);
        Touch();
    }

    public void RemoveTag()
    {
        WalletTagId = null;
        Touch();
    }

    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException(nameof(description), "Transaction description is required.");
        }

        var normalized = string.Join(' ', description.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > MaximumDescriptionLength)
        {
            throw new DomainValidationException(nameof(description), $"Transaction description cannot exceed {MaximumDescriptionLength} characters.");
        }

        return normalized;
    }

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new DomainValidationException(nameof(amount), "Transaction amount must be greater than zero.");
        }

        if (decimal.Round(amount, 2, MidpointRounding.AwayFromZero) != amount)
        {
            throw new DomainValidationException(nameof(amount), "Transaction amount cannot have more than two decimal places.");
        }

        return amount;
    }

    private static Guid? ValidateTagId(Guid? walletTagId)
    {
        if (walletTagId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(walletTagId), "Wallet tag identifier cannot be empty.");
        }

        return walletTagId;
    }

    private static string ValidateNotes(string? notes)
    {
        var normalized = notes?.Trim() ?? string.Empty;
        if (normalized.Length > MaximumNotesLength)
        {
            throw new DomainValidationException(nameof(notes), $"Transaction notes cannot exceed {MaximumNotesLength} characters.");
        }

        return normalized;
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
