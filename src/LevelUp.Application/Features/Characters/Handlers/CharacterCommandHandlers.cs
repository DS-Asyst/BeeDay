using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Characters.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Characters.Handlers;


public sealed class CreateAccountCommandHandler(
    ILevelUpRepository repository,
    IPasswordService passwordService)
    : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        Guid userId = Guid.Empty;

        await repository.UpdateAsync(data =>
        {
            var request = command.Request;
            var user = User.Create(
                request.Name,
                request.Email,
                passwordService.Hash(request.Password));

            var character = Character.Create(
                user.Id,
                request.Nickname,
                request.CharacterClass,
                request.Avatar);

            // Both entities are committed in the same repository mutation. If either
            // validation fails, no partial account is persisted.
            data.AddUser(user);
            data.AddCharacter(character);
            userId = user.Id;
        }, cancellationToken);

        return userId;
    }
}

public sealed class CreateCharacterCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<CreateCharacterCommand>
{
    public Task Handle(CreateCharacterCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var userId = CurrentUserGuard.RequireUserId(data, currentUser);
            var user = data.FindUser(userId);
            user.UpdateName(command.Request.UserName);

            if (data.FindCharacterForUser(user.Id) is not null)
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

public sealed class UpdateCurrentCharacterAvatarCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentCharacterAvatarCommand>
{
    public Task Handle(UpdateCurrentCharacterAvatarCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var userId = CurrentUserGuard.RequireUserId(data, currentUser);
            var character = data.FindCharacterForUser(userId)
                ?? throw new InvalidDomainStateException("Current Character was not found.");
            character.UpdateAvatar(command.Request.Avatar);
        }, cancellationToken);
}
