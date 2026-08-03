using BeeDay.Domain.Entities;

namespace BeeDay.Application.Common.Contracts;

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

    /// <summary>
    /// Loads the User tracked, applies <paramref name="mutation"/>, and persists the result — the
    /// missing piece identified in docs/architecture/07-persistence-contracts.md §6/§10/§13 for
    /// persisting a mutation on an already-loaded Aggregate. <paramref name="mutation"/> is pure Domain
    /// logic; the adapter never exposes any Infrastructure/EF Core concept (e.g. RowVersion) here.
    /// </summary>
    public Task UpdateAsync(Guid userId, Action<User> mutation, CancellationToken cancellationToken = default);
}
