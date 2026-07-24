using System.Security.Claims;
using LevelUp.Application.Features.Authentication.Commands;
using LevelUp.Application.Common.Contracts;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;

namespace LevelUp.Web.Services;

public sealed class AuthenticatedUserInitializer(
    AuthenticationStateProvider authenticationStateProvider,
    ISender sender,
    ILevelUpRepository repository)
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

        var data = await repository.LoadAsync(cancellationToken);
        if (data.Users.All(user => user.Id != userId))
        {
            _initializedUserId = null;
            return null;
        }

        if (_initializedUserId != userId)
        {
            await sender.Send(new SelectAuthenticatedUserCommand(userId), cancellationToken);
            _initializedUserId = userId;
        }

        return userId;
    }
}
