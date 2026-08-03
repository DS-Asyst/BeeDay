using System.Security.Claims;
using LevelUp.Application.Common.Contracts;
using Microsoft.AspNetCore.Components.Authorization;

namespace LevelUp.Web.Services;

public sealed class AuthenticatedUserInitializer(
    AuthenticationStateProvider authenticationStateProvider,
    IUserRepository repository)
{
    private Guid? _initializedUserId;

    public async Task<Guid?> EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        var value = state.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
        {
            return null;
        }

        var user = await repository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _initializedUserId = null;
            return null;
        }

        _initializedUserId = userId;

        return userId;
    }
}
