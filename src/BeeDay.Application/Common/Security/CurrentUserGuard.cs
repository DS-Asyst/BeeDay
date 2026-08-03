using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Common.Security;

public static class CurrentUserGuard
{
    /// <summary>
    /// Resolves the authenticated User's id from <paramref name="currentUser"/> only — no
    /// existence/ownership check here. The caller must validate existence and ownership explicitly
    /// through whichever Aggregate repository it calls next (its <c>GetAsync</c>/<c>GetByIdAsync</c>
    /// already encodes that check, e.g. by returning <see langword="null"/> or by matching
    /// <c>UserId</c> on the owned resource). Never falls back to a persisted document field — the
    /// Sprint 13.4-era overload that accepted <c>LevelUpData</c> to also check existence was removed on
    /// the Sprint 14.6 SQL Server cutover once its last consumer (<c>GetLevelUpQueryHandler</c>) was
    /// itself removed.
    /// </summary>
    public static Guid RequireUserId(ICurrentUserContext currentUser) =>
        currentUser.UserId ?? throw new InvalidDomainStateException("An authenticated User is required.");
}
