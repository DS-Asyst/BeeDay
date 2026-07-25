using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Common.Security;

public static class CurrentUserGuard
{
    public static Guid RequireUserId(LevelUpData data, ICurrentUserContext? context)
    {
        var userId = context?.UserId ?? data.CurrentUserId;
        if (userId is not Guid id || data.Users.All(user => user.Id != id))
        {
            throw new InvalidDomainStateException("An authenticated User is required.");
        }

        return id;
    }
}
