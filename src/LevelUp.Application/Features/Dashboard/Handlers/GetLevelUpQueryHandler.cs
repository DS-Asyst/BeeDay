using LevelUp.Application.Common.Caching;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Dashboard.Responses;
using MediatR;

namespace LevelUp.Application.Features.Dashboard.Handlers;

public sealed class GetLevelUpQueryHandler(
    ILevelUpRepository repository,
    IApplicationCache cache,
    ICurrentUserContext currentUser) : IRequestHandler<GetLevelUpQuery, GetLevelUpResponse>
{
    public async Task<GetLevelUpResponse> Handle(GetLevelUpQuery request, CancellationToken cancellationToken)
    {
        _ = cache; // Retained for DI compatibility; user data must not use a shared cache key.
        var data = await repository.LoadAsync(cancellationToken);
        var userId = CurrentUserGuard.RequireUserId(data, currentUser);
        return new GetLevelUpResponse(data.CreateUserSnapshot(userId));
    }
}
