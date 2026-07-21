using LevelUp.Application.Features.Profiles.Requests;

namespace LevelUp.Application.Features.Profiles.Contracts;

public interface IProfileService
{
    public Task SaveAsync(SaveProfileRequest request, CancellationToken cancellationToken = default);
}
