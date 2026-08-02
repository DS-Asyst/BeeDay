using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>UserToken</c> Aggregate (email confirmation and password reset
/// tokens). Deliberately independent from <see cref="IUserRepository"/> — see
/// docs/architecture/05-domain-aggregate-map.md §2.2 for why token revocation does not require
/// <c>User</c> containment. The password-reset consumption flow is the one place this port and
/// <see cref="IUserRepository"/> need to be written together — documented, not implemented, in
/// docs/architecture/07-persistence-contracts.md.
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
}
