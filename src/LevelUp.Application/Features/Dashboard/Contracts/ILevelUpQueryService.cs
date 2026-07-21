using LevelUp.Application.Features.Dashboard.Responses;

namespace LevelUp.Application.Features.Dashboard.Contracts;

public interface ILevelUpQueryService
{
    public Task<GetLevelUpResponse> GetAsync(CancellationToken cancellationToken = default);
}
