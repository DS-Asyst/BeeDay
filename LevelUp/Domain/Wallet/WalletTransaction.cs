using System.Text.Json.Serialization;

namespace LevelUp.Domain.Wallet;

public sealed class WalletTransaction
{
    public int Id { get; set; }

    [JsonInclude]
    public WalletTransactionType Type { get; private set; }

    [JsonInclude]
    public decimal Amount { get; private set; }

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public string Justification { get; private set; } = string.Empty;

    [JsonInclude]
    public DateTime OccurredAt { get; private set; }

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [JsonInclude]
    public DateTime? UpdatedAt { get; private set; }

    public void Configure(
        WalletTransactionType type,
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        if (Amount > 0)
        {
            throw new InvalidOperationException(
                "A movimentação já foi configurada."
            );
        }

        Apply(type, amount, description, justification, occurredAt);
    }

    public void UpdateDetails(
        WalletTransactionType type,
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        Apply(type, amount, description, justification, occurredAt);
        UpdatedAt = DateTime.Now;
    }

    private void Apply(
        WalletTransactionType type,
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "O valor da movimentação deve ser maior que zero."
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (type == WalletTransactionType.Withdrawal)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(justification);
        }

        Type = type;
        Amount = amount;
        Description = description.Trim();
        Justification = type == WalletTransactionType.Withdrawal
            ? justification.Trim()
            : string.Empty;
        OccurredAt = occurredAt.Date;
    }
}
