
namespace LevelUp.Domain.Wallet;

public sealed class WalletTransaction
{
    public int Id { get; set; }

    // Mantido para compatibilidade com saves anteriores. Novas regras usam Amount assinado.
    public WalletTransactionType Type { get; private set; }

    public decimal Amount { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public int? TagId { get; private set; }

    public string Justification { get; private set; } = string.Empty;

    public DateTime OccurredAt { get; private set; }

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? UpdatedAt { get; private set; }

    public int? ReversalOfTransactionId { get; private set; }

    public DateTime? ReversedAt { get; private set; }

    public string ReversalReason { get; private set; } = string.Empty;

    public bool IsReversal => ReversalOfTransactionId is not null;
    public bool IsReversed => ReversedAt is not null;
    public bool IsCredit => Amount > 0;
    public bool IsDebit => Amount < 0;

    public void Configure(
        decimal amount,
        string description,
        int tagId,
        DateTime occurredAt
    )
    {
        if (Amount != 0)
        {
            throw new InvalidOperationException("A movimentação já foi configurada.");
        }

        Apply(amount, description, tagId, occurredAt);
    }

    public void Configure(
        WalletTransactionType type,
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        if (Amount != 0)
        {
            throw new InvalidOperationException("A movimentação já foi configurada.");
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Amount = amount;
        Type = type;
        Description = description.Trim();
        Justification = justification?.Trim() ?? string.Empty;
        OccurredAt = occurredAt.Date;
    }

    public void UpdateDetails(
        decimal amount,
        string description,
        int tagId,
        DateTime occurredAt
    )
    {
        Apply(amount, description, tagId, occurredAt);
        UpdatedAt = DateTime.Now;
    }

    public void AssignTagForMigration(int tagId)
    {
        if (tagId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tagId));
        }

        TagId = tagId;
        Justification = string.Empty;
    }

    public void ConvertLegacyAmountToSigned()
    {
        if (Amount == 0)
        {
            return;
        }

        if (Type == WalletTransactionType.Withdrawal && Amount > 0)
        {
            Amount = -Amount;
        }

        Type = Amount >= 0
            ? WalletTransactionType.Deposit
            : WalletTransactionType.Withdrawal;
    }

    public void ConfigureReversal(
        WalletTransaction original,
        string reason,
        DateTime occurredAt
    )
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Apply(
            -original.Amount,
            $"Estorno: {original.Description}",
            original.TagId,
            occurredAt
        );

        ReversalOfTransactionId = original.Id;
        ReversalReason = reason.Trim();
        original.MarkReversed(occurredAt);
    }

    private void MarkReversed(DateTime reversedAt)
    {
        if (IsReversed)
        {
            throw new InvalidOperationException("A movimentação já foi estornada.");
        }

        ReversedAt = reversedAt;
    }

    private void Apply(
        decimal amount,
        string description,
        int? tagId,
        DateTime occurredAt
    )
    {
        if (amount == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "O valor da movimentação não pode ser zero."
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (tagId.HasValue && tagId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tagId));
        }

        Amount = amount;
        Type = amount > 0
            ? WalletTransactionType.Deposit
            : WalletTransactionType.Withdrawal;
        Description = description.Trim();
        TagId = tagId;
        Justification = string.Empty;
        OccurredAt = occurredAt.Date;
    }
}
