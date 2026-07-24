using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Characters.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Characters.Handlers;

public sealed class CreateCharacterCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<CreateCharacterCommand>
{
    public Task Handle(CreateCharacterCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.CurrentUser;
            if (user is null)
            {
                user = User.Create(
                    command.Request.UserName,
                    $"onboarding-{Guid.NewGuid():N}@levelup.invalid");
                data.AddUser(user);
                data.SetCurrentUser(user.Id);
            }
            else
            {
                user.UpdateName(command.Request.UserName);
            }

            if (data.CurrentCharacter is not null)
            {
                throw new InvalidDomainStateException("The current User already has a Character.");
            }

            data.AddCharacter(Character.Create(
                user.Id,
                command.Request.Nickname,
                command.Request.CharacterClass,
                command.Request.Avatar));
        }, cancellationToken);
}

public sealed class UpdateCurrentCharacterAvatarCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentCharacterAvatarCommand>
{
    public Task Handle(UpdateCurrentCharacterAvatarCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var character = data.CurrentCharacter
                ?? throw new InvalidDomainStateException("Current Character was not found.");
            character.UpdateAvatar(command.Request.Avatar);
        }, cancellationToken);
}
