using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Common.Security;

public static class CurrentUserGuard
{
    /// <summary>
    /// Resolves the authenticated User's id. Depends exclusively on the authenticated identity
    /// (<paramref name="currentUser"/>) — never on <c>LevelUpData.CurrentUserId</c>, which is a
    /// persisted document-bootstrapping field, not an authentication mechanism.
    /// </summary>
    /// <remarks>
    /// Legacy overload for handlers not yet migrated to a per-Aggregate persistence contract
    /// (Sprint 13.4 — see docs/architecture/07-persistence-contracts.md). Removed once every handler
    /// uses <see cref="RequireUserId(ICurrentUserContext)"/> instead.
    /// </remarks>
    public static Guid RequireUserId(LevelUpData data, ICurrentUserContext currentUser)
    {
        if (currentUser.UserId is not Guid id || data.Users.All(user => user.Id != id))
        {
            throw new InvalidDomainStateException("An authenticated User is required.");
        }

        return id;
    }

    /// <summary>
    /// Resolves the authenticated User's id from <paramref name="currentUser"/> only — no
    /// existence/ownership check here. The caller must validate existence and ownership explicitly
    /// through whichever Aggregate repository it calls next (its <c>GetAsync</c>/<c>GetByIdAsync</c>
    /// already encodes that check, e.g. by returning <see langword="null"/> or by matching
    /// <c>UserId</c> on the owned resource). Never falls back to a persisted document field.
    /// </summary>
    public static Guid RequireUserId(ICurrentUserContext currentUser) =>
        currentUser.UserId ?? throw new InvalidDomainStateException("An authenticated User is required.");
}
