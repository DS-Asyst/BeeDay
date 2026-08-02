using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>User</c> Aggregate (identity, profile, session state, and the
/// embedded XP/level history). See docs/architecture/07-persistence-contracts.md for the ownership
/// and consistency rationale — this port never accepts or returns <c>LevelUpData</c>.
/// </summary>
public interface IUserRepository
{
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    public Task<bool> IsEmailInUseAsync(
        string normalizedEmail,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default);

    public Task<bool> IsNicknameInUseAsync(
        string normalizedNickname,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default);

    public Task AddAsync(User user, CancellationToken cancellationToken = default);
}
