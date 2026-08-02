using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>Transaction</c> Aggregate — already independent from
/// <c>Wallet</c> in the Domain today (no containment), confirmed in
/// docs/architecture/05-domain-aggregate-map.md §2.7. Filtered/paginated/sorted transaction listings
/// for display belong to <c>IWalletReadService</c>, not this port — loading every Transaction for a
/// wallet just to filter/sort/paginate in memory is exactly what read services exist to avoid
/// (docs/architecture/02-target-architecture.md §4). <see cref="ListByTagAsync"/> exists to support
/// clearing tag references when a <c>WalletTag</c> is deleted (see
/// docs/architecture/06-domain-persistence-map.md §2.8).
/// </summary>
public interface ITransactionRepository
{
    public Task<Transaction?> GetAsync(
        Guid walletId,
        Guid transactionId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Transaction>> ListByTagAsync(
        Guid walletTagId,
        CancellationToken cancellationToken = default);

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    public Task RemoveAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
