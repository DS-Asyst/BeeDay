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
    public static Guid RequireUserId(LevelUpData data, ICurrentUserContext currentUser)
    {
        if (currentUser.UserId is not Guid id || data.Users.All(user => user.Id != id))
        {
            throw new InvalidDomainStateException("An authenticated User is required.");
        }

        return id;
    }
}
