using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Queries;
using LevelUp.Application.Features.Users.Responses;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Users.Handlers;

public sealed class CreateUserCommandHandler(
    ILevelUpRepository repository,
    IPasswordService passwordService) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var passwordHash = string.IsNullOrWhiteSpace(command.Request.Password)
                ? null
                : passwordService.Hash(command.Request.Password);

            var user = User.Create(command.Request.Name, command.Request.Email, passwordHash);
            data.AddUser(user);
            data.SetCurrentUser(user.Id);
            id = user.Id;
        }, cancellationToken);
        return id;
    }
}

public sealed class UpdateCurrentUserPreferencesCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentUserPreferencesCommand>
{
    public Task Handle(UpdateCurrentUserPreferencesCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.FindUser(CurrentUserGuard.RequireUserId(data, currentUser));
            user.UpdatePreferences(command.Request.Language, command.Request.Theme);
        }, cancellationToken);
}

public sealed class UpdateCurrentUserAccountCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<UpdateCurrentUserAccountCommand>
{
    public Task Handle(UpdateCurrentUserAccountCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.FindUser(CurrentUserGuard.RequireUserId(data, currentUser));
            if (data.Users.Any(candidate => candidate.Id != user.Id &&
                string.Equals(candidate.Email, command.Request.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDomainStateException($"Email '{command.Request.Email}' is already registered.");
            }
            user.UpdateAccount(command.Request.Name, command.Request.Email);
        }, cancellationToken);
}

public sealed class ChangeCurrentUserPasswordCommandHandler(
    ILevelUpRepository repository,
    IPasswordService passwordService,
    ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<ChangeCurrentUserPasswordCommand>
{
    public Task Handle(ChangeCurrentUserPasswordCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.FindUser(CurrentUserGuard.RequireUserId(data, currentUser));

            if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
                !passwordService.Verify(command.Request.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidDomainStateException("The current password is incorrect.");
            }

            if (passwordService.Verify(command.Request.NewPassword, user.PasswordHash))
            {
                throw new InvalidDomainStateException("The new password must be different from the current password.");
            }

            user.SetPasswordHash(passwordService.Hash(command.Request.NewPassword));
        }, cancellationToken);
}


public sealed class CompleteCurrentUserOnboardingCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<CompleteCurrentUserOnboardingCommand>
{
    public Task Handle(CompleteCurrentUserOnboardingCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = data.FindUser(CurrentUserGuard.RequireUserId(data, currentUser));
            user.CompleteOnboarding();
        }, cancellationToken);
}

public sealed class GetCurrentUserQueryHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse?>
{
    public async Task<CurrentUserResponse?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.LoadAsync(cancellationToken);
        var userId = CurrentUserGuard.RequireUserId(data, currentUser);
        var user = data.FindUser(userId);
        return new(user.Id, user.Name, user.Email, user.Language, user.Theme, user.IsActive, user.HasCompletedOnboarding);
    }
}

public sealed class GetCurrentCharacterQueryHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : IRequestHandler<GetCurrentCharacterQuery, CurrentCharacterResponse?>
{
    public async Task<CurrentCharacterResponse?> Handle(GetCurrentCharacterQuery request, CancellationToken cancellationToken)
    {
        var data = await repository.LoadAsync(cancellationToken);
        var userId = CurrentUserGuard.RequireUserId(data, currentUser);
        var character = data.FindCharacterForUser(userId);
        return character is null ? null : new(character.Id, character.UserId, character.Nickname, character.Class, character.Avatar);
    }
}
