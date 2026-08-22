using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;

namespace BeeDay.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>UserToken</c> Aggregate (email confirmation and password reset
/// tokens). Deliberately independent from <see cref="IUserRepository"/> — see
/// docs/history/domain-aggregate-map.md §2.2 for why token revocation does not require
/// <c>User</c> containment. The password-reset consumption flow is the one place this port and
/// <see cref="IUserRepository"/> need to be written together — documented, not implemented, in
/// docs/history/persistence-contracts.md.
/// </summary>
public interface IUserTokenRepository
{
    public Task<UserToken?> GetByHashAsync(
        string tokenHash,
        UserTokenType type,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<UserToken>> ListActiveAsync(
        Guid userId,
        UserTokenType type,
        CancellationToken cancellationToken = default);

    public Task AddAsync(UserToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the UserToken tracked by its own <paramref name="tokenId"/>, applies
    /// <paramref name="mutation"/>, and persists the result (e.g. <c>MarkAsUsed</c>) — see
    /// <see cref="IUserRepository.UpdateAsync"/> for why this shape, not a disconnected "Save".
    /// </summary>
    public Task UpdateAsync(Guid tokenId, Action<UserToken> mutation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes every currently-active token of <paramref name="type"/> for <paramref name="userId"/> —
    /// approved in docs/history/persistence-contracts.md §10, implemented here. Used before
    /// issuing a fresh token of the same type, so no two tokens of that type are ever simultaneously
    /// active for the same User.
    /// </summary>
    public Task RevokeActiveAsync(
        Guid userId,
        UserTokenType type,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default);
}
