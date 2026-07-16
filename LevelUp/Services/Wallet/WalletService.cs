using LevelUp.Domain.Wallet;

namespace LevelUp.Services.Wallet;

public sealed class WalletService
{
    private readonly List<WalletTransaction> transactions = [];
    private int nextId = 1;

    public WalletService(IEnumerable<WalletTransaction>? transactions = null)
    {
        if (transactions is null)
        {
            return;
        }

        this.transactions.AddRange(transactions);
        nextId = this.transactions.Count == 0
            ? 1
            : this.transactions.Max(transaction => transaction.Id) + 1;
    }

    public decimal Balance => transactions.Sum(GetSignedAmount);

    public WalletTransaction AddDeposit(
        decimal amount,
        string description,
        DateTime occurredAt
    )
    {
        return CreateTransaction(
            WalletTransactionType.Deposit,
            amount,
            description,
            string.Empty,
            occurredAt
        );
    }

    public WalletTransaction AddWithdrawal(
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        if (amount > Balance)
        {
            throw new InvalidOperationException(
                "A retirada não pode ser maior que o saldo disponível."
            );
        }

        return CreateTransaction(
            WalletTransactionType.Withdrawal,
            amount,
            description,
            justification,
            occurredAt
        );
    }

    public IReadOnlyList<WalletTransaction> GetAll()
    {
        return transactions
            .OrderByDescending(transaction => transaction.OccurredAt)
            .ThenByDescending(transaction => transaction.Id)
            .ToList()
            .AsReadOnly();
    }

    public WalletTransaction? GetById(int id)
    {
        return transactions.FirstOrDefault(
            transaction => transaction.Id == id
        );
    }

    public void UpdateTransaction(
        WalletTransaction transaction,
        WalletTransactionType type,
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        EnsureManaged(transaction);

        decimal balanceWithoutTransaction =
            Balance - GetSignedAmount(transaction);

        if (type == WalletTransactionType.Withdrawal &&
            amount > balanceWithoutTransaction)
        {
            throw new InvalidOperationException(
                "A retirada não pode ser maior que o saldo disponível."
            );
        }

        transaction.UpdateDetails(
            type,
            amount,
            description,
            justification,
            occurredAt
        );
    }

    public bool DeleteTransaction(int id)
    {
        WalletTransaction? transaction = GetById(id);

        if (transaction is null)
        {
            return false;
        }

        decimal resultingBalance = Balance - GetSignedAmount(transaction);

        if (resultingBalance < 0)
        {
            throw new InvalidOperationException(
                "Esta movimentação não pode ser excluída porque deixaria o saldo negativo."
            );
        }

        return transactions.Remove(transaction);
    }

    public decimal GetMonthlyBalance(int year, int month)
    {
        return transactions
            .Where(transaction =>
                transaction.OccurredAt.Year == year &&
                transaction.OccurredAt.Month == month
            )
            .Sum(GetSignedAmount);
    }

    private WalletTransaction CreateTransaction(
        WalletTransactionType type,
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        WalletTransaction transaction = new()
        {
            Id = nextId++
        };

        transaction.Configure(
            type,
            amount,
            description,
            justification,
            occurredAt
        );

        transactions.Add(transaction);
        return transaction;
    }

    private static decimal GetSignedAmount(
        WalletTransaction transaction
    )
    {
        return transaction.Type == WalletTransactionType.Deposit
            ? transaction.Amount
            : -transaction.Amount;
    }

    private void EnsureManaged(WalletTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (!transactions.Any(existing => existing.Id == transaction.Id))
        {
            throw new InvalidOperationException(
                "A movimentação não é gerenciada por este serviço."
            );
        }
    }
}
