using BeeDay.Domain.Entities;

namespace BeeDay.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>Wallet</c> Aggregate. Deliberately thin — Wallet owns no child
/// entities and stores no balance; balance is always computed from externally supplied Transactions
/// (see docs/architecture/06-domain-persistence-map.md §2.6).
/// </summary>
public interface IWalletRepository
{
    public Task<Wallet?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the current User's Wallet tracked, applies <paramref name="mutation"/>, and persists the
    /// result (e.g. <c>Touch()</c> after a Transaction change) — see
    /// <see cref="IUserRepository.UpdateAsync"/> for why this shape.
    /// </summary>
    public Task UpdateAsync(Guid userId, Action<Wallet> mutation, CancellationToken cancellationToken = default);
}
