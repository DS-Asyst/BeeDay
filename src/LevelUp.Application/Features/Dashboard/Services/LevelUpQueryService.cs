using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Features.Dashboard.Contracts;
using LevelUp.Application.Features.Dashboard.Responses;

namespace LevelUp.Application.Features.Dashboard.Services;

public sealed class LevelUpQueryService(ILevelUpRepository repository) : ILevelUpQueryService
{
    public async Task<GetLevelUpResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var data = await repository.LoadAsync(cancellationToken);
        return new GetLevelUpResponse(data);
    }
}
