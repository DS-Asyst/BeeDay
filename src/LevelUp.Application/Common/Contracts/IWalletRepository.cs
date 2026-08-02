using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>Wallet</c> Aggregate. Deliberately thin — Wallet owns no child
/// entities and stores no balance; balance is always computed from externally supplied Transactions
/// (see docs/architecture/06-domain-persistence-map.md §2.6).
/// </summary>
public interface IWalletRepository
{
    public Task<Wallet?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
}
