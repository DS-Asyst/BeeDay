using LevelUp.Application.Common.Caching;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Events;
using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Dashboard.Responses;
using MediatR;

namespace LevelUp.Application.Features.Dashboard.Handlers;

public sealed class GetLevelUpQueryHandler(
    ILevelUpRepository repository,
    IApplicationCache cache) : IRequestHandler<GetLevelUpQuery, GetLevelUpResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public Task<GetLevelUpResponse> Handle(
        GetLevelUpQuery request,
        CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            CacheKeys.Dashboard,
            async token => new GetLevelUpResponse(await repository.LoadAsync(token)),
            CacheDuration,
            cancellationToken);
}
