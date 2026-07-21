using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Profiles.Commands;
using LevelUp.Domain.Entities;
using MediatR;

namespace LevelUp.Application.Features.Profiles.Handlers;

public sealed class SaveProfileCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<SaveProfileCommand>
{
    public Task Handle(SaveProfileCommand command, CancellationToken cancellationToken)
    {
        return MutateAsync(data =>
        {
            var request = command.Request;

            if (data.Profile is null)
            {
                data.SetProfile(Profile.Create(
                    request.Name,
                    request.Nickname,
                    request.CharacterClass));
            }
            else
            {
                data.Profile.Update(
                    request.Name,
                    request.Nickname,
                    request.CharacterClass);
            }
        }, cancellationToken);
    }
}
