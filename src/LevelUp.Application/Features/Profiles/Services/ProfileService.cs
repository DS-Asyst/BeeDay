using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Services;
using LevelUp.Application.Features.Profiles.Contracts;
using LevelUp.Application.Features.Profiles.Requests;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Features.Profiles.Services;

public sealed class ProfileService(ILevelUpRepository repository)
    : ApplicationService(repository), IProfileService
{
    public Task SaveAsync(SaveProfileRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data =>
        {
            if (data.Profile is null)
            {
                data.SetProfile(Profile.Create(request.Name, request.Nickname, request.CharacterClass));
                return;
            }

            data.Profile.Update(request.Name, request.Nickname, request.CharacterClass);
        }, cancellationToken);
}
