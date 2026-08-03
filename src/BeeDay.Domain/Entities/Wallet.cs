using BeeDay.Domain.Abstractions;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.Entities;

public sealed class Wallet : Entity
{
    public Guid UserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public static Wallet Create(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(userId), "User identifier is required.");
        }

        return new Wallet { UserId = userId };
    }

    public decimal CalculateBalance(IEnumerable<Transaction> transactions) =>
        FilterTransactions(transactions).Sum(transaction => transaction.SignedAmount);

    public decimal CalculateTotalIncome(IEnumerable<Transaction> transactions) =>
        FilterTransactions(transactions)
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount);

    public decimal CalculateTotalExpenses(IEnumerable<Transaction> transactions) =>
        FilterTransactions(transactions)
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);

    public void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;

    private IEnumerable<Transaction> FilterTransactions(IEnumerable<Transaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        return transactions.Where(transaction => transaction.WalletId == Id);
    }
}
