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

public sealed class UpdateCurrentCharacterCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentCharacterCommand>
{
    public Task Handle(UpdateCurrentCharacterCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var character = data.CurrentCharacter
                ?? throw new InvalidDomainStateException("Current Character was not found.");
            if (data.Characters.Any(candidate => candidate.Id != character.Id &&
                string.Equals(candidate.Nickname, command.Request.Nickname, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDomainStateException($"Nickname '@{command.Request.Nickname}' is already in use.");
            }
            character.UpdateNicknameAndAvatar(command.Request.Nickname, command.Request.Avatar);
        }, cancellationToken);
}
