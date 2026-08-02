using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>WalletTag</c> Aggregate. Owned by <c>UserId</c>, not
/// <c>WalletId</c> — the Domain has no <c>Wallet</c> reference on <c>WalletTag</c> at all (see
/// docs/architecture/05-domain-aggregate-map.md §2.8; docs/data/01-relational-model.md models this
/// aggregate incorrectly today and needs correction whenever the SQL schema is drafted, per
/// docs/architecture/06-domain-persistence-map.md §0).
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
}
