using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Queries;
using LevelUp.Application.Features.Users.Responses;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Users.Handlers;

public sealed class CreateUserCommandHandler(ILevelUpRepository repository) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = User.Create(command.Request.Name, command.Request.Email, command.Request.PasswordHash);
            data.AddUser(user);
            data.SetCurrentUser(user.Id);
            id = user.Id;
        }, cancellationToken);
        return id;
    }
}

public sealed class UpdateCurrentUserPreferencesCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentUserPreferencesCommand>
{
    public Task Handle(UpdateCurrentUserPreferencesCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.CurrentUser ?? throw new InvalidDomainStateException("Current User was not found.");
            user.UpdatePreferences(command.Request.Language, command.Request.Theme);
        }, cancellationToken);
}

public sealed class UpdateCurrentUserAccountCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentUserAccountCommand>
{
    public Task Handle(UpdateCurrentUserAccountCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.CurrentUser ?? throw new InvalidDomainStateException("Current User was not found.");
            if (data.Users.Any(candidate => candidate.Id != user.Id &&
                string.Equals(candidate.Email, command.Request.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDomainStateException($"Email '{command.Request.Email}' is already registered.");
            }
            user.UpdateAccount(command.Request.Name, command.Request.Email);
        }, cancellationToken);
}

public sealed class GetCurrentUserQueryHandler(ILevelUpRepository repository)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse?>
{
    public async Task<CurrentUserResponse?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = (await repository.LoadAsync(cancellationToken)).CurrentUser;
        return user is null ? null : new(user.Id, user.Name, user.Email, user.Language, user.Theme, user.IsActive);
    }
}

public sealed class GetCurrentCharacterQueryHandler(ILevelUpRepository repository)
    : IRequestHandler<GetCurrentCharacterQuery, CurrentCharacterResponse?>
{
    public async Task<CurrentCharacterResponse?> Handle(GetCurrentCharacterQuery request, CancellationToken cancellationToken)
    {
        var character = (await repository.LoadAsync(cancellationToken)).CurrentCharacter;
        return character is null ? null : new(character.Id, character.UserId, character.Nickname, character.Class, character.Avatar);
    }
}
