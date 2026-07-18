using LevelUp.Domain.Wallet;

namespace LevelUp.Services.Wallet;

public sealed class WalletService
{
    private readonly List<WalletTransaction> transactions = [];
    private readonly List<WalletTag> tags = [];
    private int nextTransactionId = 1;
    private int nextTagId = 1;

    public WalletService(
        IEnumerable<WalletTransaction>? transactions = null,
        IEnumerable<WalletTag>? tags = null
    )
    {
        if (transactions is not null)
        {
            this.transactions.AddRange(transactions);
            nextTransactionId = this.transactions.Count == 0
                ? 1
                : this.transactions.Max(transaction => transaction.Id) + 1;
        }

        if (tags is not null)
        {
            this.tags.AddRange(tags);
            nextTagId = this.tags.Count == 0
                ? 1
                : this.tags.Max(tag => tag.Id) + 1;
        }
    }

    public decimal Balance => transactions.Sum(transaction => transaction.Amount);

    public WalletTag CreateTag(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (tags.Any(tag => string.Equals(
            tag.Name,
            name.Trim(),
            StringComparison.OrdinalIgnoreCase
        )))
        {
            throw new InvalidOperationException("A tag with this name already exists.");
        }

        WalletTag tag = new() { Id = nextTagId++ };
        tag.Configure(name);
        tags.Add(tag);
        return tag;
    }

    public void UpdateTag(WalletTag tag, string name)
    {
        EnsureManagedTag(tag);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (tags.Any(existing =>
            existing.Id != tag.Id &&
            string.Equals(existing.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A tag with this name already exists.");
        }

        tag.UpdateName(name);
    }

    public bool DeleteTag(int id)
    {
        WalletTag? tag = GetTagById(id);
        if (tag is null)
        {
            return false;
        }

        if (transactions.Any(transaction => transaction.TagId == id))
        {
            throw new InvalidOperationException(
                "Tags used by transactions cannot be deleted."
            );
        }

        return tags.Remove(tag);
    }

    public IReadOnlyList<WalletTag> GetAllTags() => tags
        .OrderBy(tag => tag.Name)
        .ToList()
        .AsReadOnly();

    public WalletTag? GetTagById(int id) =>
        tags.FirstOrDefault(tag => tag.Id == id);

    public string GetTagName(int? tagId)
    {
        if (tagId is null)
        {
            return "No tag";
        }

        return GetTagById(tagId.Value)?.Name ?? "Tag not found";
    }

    public WalletTransaction AddTransaction(
        decimal amount,
        string description,
        WalletTag tag,
        DateTime? occurredAt = null
    )
    {
        EnsureManagedTag(tag);

        WalletTransaction transaction = new() { Id = nextTransactionId++ };
        transaction.Configure(
            amount,
            description,
            tag.Id,
            occurredAt ?? DateTime.Now
        );
        transactions.Add(transaction);
        return transaction;
    }

    // Compatibilidade temporária com chamadas anteriores.
    public WalletTransaction AddEntry(
        decimal amount,
        string description,
        WalletTag tag,
        DateTime occurredAt
    ) => AddTransaction(Math.Abs(amount), description, tag, occurredAt);

    public WalletTransaction AddExit(
        decimal amount,
        string description,
        WalletTag tag,
        DateTime occurredAt
    ) => AddTransaction(-Math.Abs(amount), description, tag, occurredAt);

    public WalletTransaction AddDeposit(
        decimal amount,
        string description,
        DateTime occurredAt
    )
    {
        WalletTag tag = GetOrCreateLegacyTag();
        return AddTransaction(Math.Abs(amount), description, tag, occurredAt);
    }

    public WalletTransaction AddWithdrawal(
        decimal amount,
        string description,
        string justification,
        DateTime occurredAt
    )
    {
        WalletTag tag = GetOrCreateLegacyTag();
        return AddTransaction(-Math.Abs(amount), description, tag, occurredAt);
    }

    public IReadOnlyList<WalletTransaction> GetAll() => transactions
        .OrderByDescending(transaction => transaction.OccurredAt)
        .ThenByDescending(transaction => transaction.Id)
        .ToList()
        .AsReadOnly();

    public WalletTransaction? GetById(int id) =>
        transactions.FirstOrDefault(transaction => transaction.Id == id);

    public bool DeleteTransaction(int id)
    {
        WalletTransaction? transaction = GetById(id);
        return transaction is not null && transactions.Remove(transaction);
    }

    public WalletTransaction ReverseTransaction(
        WalletTransaction transaction,
        string reason,
        DateTime? occurredAt = null
    )
    {
        EnsureManaged(transaction);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (transaction.IsReversal)
        {
            throw new InvalidOperationException(
                "A reversal transaction cannot be reversed again."
            );
        }

        if (transaction.IsReversed)
        {
            throw new InvalidOperationException("The transaction has already been reversed.");
        }

        WalletTransaction reversal = new() { Id = nextTransactionId++ };
        reversal.ConfigureReversal(transaction, reason, occurredAt ?? DateTime.Now);
        transactions.Add(reversal);
        return reversal;
    }

    public decimal GetMonthlyBalance(int year, int month) => transactions
        .Where(transaction =>
            transaction.OccurredAt.Year == year &&
            transaction.OccurredAt.Month == month)
        .Sum(transaction => transaction.Amount);

    private WalletTag GetOrCreateLegacyTag()
    {
        WalletTag? existing = tags.FirstOrDefault(tag =>
            string.Equals(tag.Name, "No tag", StringComparison.OrdinalIgnoreCase));

        return existing ?? CreateTag("No tag");
    }

    private void EnsureManaged(WalletTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (!transactions.Any(existing => existing.Id == transaction.Id))
        {
            throw new InvalidOperationException(
                "The transaction is not managed by this service."
            );
        }
    }

    private void EnsureManagedTag(WalletTag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (!tags.Any(existing => existing.Id == tag.Id))
        {
            throw new InvalidOperationException("The tag is not managed by this service.");
        }
    }
}
