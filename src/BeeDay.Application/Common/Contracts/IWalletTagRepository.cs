using BeeDay.Domain.Entities;

namespace BeeDay.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>WalletTag</c> Aggregate. Owned by <c>UserId</c>, not
/// <c>WalletId</c> — the Domain has no <c>Wallet</c> reference on <c>WalletTag</c> at all (see
/// docs/history/domain-aggregate-map.md §2.8; docs/persistence/01-relational-model.md models this
/// aggregate incorrectly today and needs correction whenever the SQL schema is drafted, per
/// docs/history/domain-persistence-map.md §0).
/// </summary>
public interface IWalletTagRepository
{
    public Task<WalletTag?> GetAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<WalletTag>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task<bool> IsNameInUseAsync(
        Guid userId,
        string normalizedName,
        Guid? excludingTagId = null,
        CancellationToken cancellationToken = default);

    public Task AddAsync(WalletTag tag, CancellationToken cancellationToken = default);

    public Task RemoveAsync(WalletTag tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the WalletTag tracked, applies <paramref name="mutation"/>, and persists the result — see
    /// <see cref="IUserRepository.UpdateAsync"/> for why this shape, not a disconnected "Save".
    /// </summary>
    public Task UpdateAsync(
        Guid userId,
        Guid tagId,
        Action<WalletTag> mutation,
        CancellationToken cancellationToken = default);
}
