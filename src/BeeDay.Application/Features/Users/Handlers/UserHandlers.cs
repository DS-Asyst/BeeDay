using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Identity;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Queries;
using LevelUp.Application.Features.Users.Responses;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using LevelUp.Domain.ValueObjects;
using MediatR;

namespace LevelUp.Application.Features.Users.Handlers;

public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    IEmailConfirmationIssuer confirmationIssuer,
    IEmailSender emailSender) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        Guid id;
        EmailMessage confirmationEmail;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var request = command.Request;
            var passwordHash = string.IsNullOrWhiteSpace(request.Password) ? null : passwordService.Hash(request.Password);
            var user = User.Create(request.Name, request.Email, passwordHash);

            if (await unitOfWork.Users.IsEmailInUseAsync(user.Email, excludingUserId: null, cancellationToken))
            {
                throw new InvalidDomainStateException($"Email '{user.Email}' is already registered.");
            }

            await unitOfWork.Users.AddAsync(user, cancellationToken);

            var (token, message) = confirmationIssuer.Issue(user);
            await unitOfWork.UserTokens.AddAsync(token, cancellationToken);

            id = user.Id;
            confirmationEmail = message;

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }

        await emailSender.SendAsync(confirmationEmail, cancellationToken);
        return id;
    }
}

public sealed class CreateAccountCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    IEmailConfirmationIssuer confirmationIssuer,
    IEmailSender emailSender) : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        Guid userId;
        EmailMessage confirmationEmail;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var request = command.Request;
            var user = User.Create(request.Name, request.Email, passwordService.Hash(request.Password));

            if (await unitOfWork.Users.IsEmailInUseAsync(user.Email, excludingUserId: null, cancellationToken))
            {
                throw new InvalidDomainStateException($"Email '{user.Email}' is already registered.");
            }

            var normalizedNickname = Nickname.Create(request.Nickname).Value;
            if (await unitOfWork.Users.IsNicknameInUseAsync(normalizedNickname, excludingUserId: null, cancellationToken))
            {
                throw new InvalidDomainStateException($"Nickname '@{normalizedNickname}' is already in use.");
            }

            // Both mutations are applied to the same in-memory User before it is ever persisted, so a
            // single AddAsync commits them together — no partial account can be observed.
            user.CompleteProfile(request.Nickname, request.Avatar);

            await unitOfWork.Users.AddAsync(user, cancellationToken);

            var (token, message) = confirmationIssuer.Issue(user);
            await unitOfWork.UserTokens.AddAsync(token, cancellationToken);

            userId = user.Id;
            confirmationEmail = message;

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }

        await emailSender.SendAsync(confirmationEmail, cancellationToken);
        return userId;
    }
}

public sealed class CompleteUserProfileCommandHandler(IUserRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<CompleteUserProfileCommand>
{
    public async Task Handle(CompleteUserProfileCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var user = await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);

        if (user.HasProfile)
        {
            throw new InvalidDomainStateException("A User can only complete their profile once.");
        }

        var request = command.Request;
        var normalizedNickname = Nickname.Create(request.Nickname).Value;
        if (await repository.IsNicknameInUseAsync(normalizedNickname, userId, cancellationToken))
        {
            throw new InvalidDomainStateException($"Nickname '@{normalizedNickname}' is already in use.");
        }

        await repository.UpdateAsync(userId, u =>
        {
            u.UpdateName(request.FullName);
            u.CompleteProfile(request.Nickname, request.Avatar);
        }, cancellationToken);
    }
}

public sealed class UpdateCurrentUserAvatarCommandHandler(IUserRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<UpdateCurrentUserAvatarCommand>
{
    public async Task Handle(UpdateCurrentUserAvatarCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);
        await repository.UpdateAsync(userId, user => user.UpdateAvatar(command.Request.Avatar), cancellationToken);
    }
}

public sealed class UpdateCurrentUserPreferencesCommandHandler(IUserRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<UpdateCurrentUserPreferencesCommand>
{
    public async Task Handle(UpdateCurrentUserPreferencesCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);
        await repository.UpdateAsync(
            userId,
            user => user.UpdatePreferences(command.Request.Language, command.Request.Theme),
            cancellationToken);
    }
}

public sealed class UpdateCurrentUserAccountCommandHandler(IUserRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<UpdateCurrentUserAccountCommand>
{
    public async Task Handle(UpdateCurrentUserAccountCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);

        var request = command.Request;
        if (await repository.IsEmailInUseAsync(request.Email.Trim(), userId, cancellationToken))
        {
            throw new InvalidDomainStateException($"Email '{request.Email}' is already registered.");
        }

        await repository.UpdateAsync(userId, user => user.UpdateAccount(request.Name, request.Email), cancellationToken);
    }
}

public sealed class ChangeCurrentUserPasswordCommandHandler(
    IUserRepository repository,
    IPasswordService passwordService,
    ICurrentUserContext currentUser) : IRequestHandler<ChangeCurrentUserPasswordCommand>
{
    public async Task Handle(ChangeCurrentUserPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var user = await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordService.Verify(command.Request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidDomainStateException("The current password is incorrect.");
        }

        if (passwordService.Verify(command.Request.NewPassword, user.PasswordHash))
        {
            throw new InvalidDomainStateException("The new password must be different from the current password.");
        }

        await repository.UpdateAsync(userId, u =>
        {
            u.SetPasswordHash(passwordService.Hash(command.Request.NewPassword));
            u.InvalidateSessions();
        }, cancellationToken);
    }
}

public sealed class CompleteCurrentUserOnboardingCommandHandler(IUserRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<CompleteCurrentUserOnboardingCommand>
{
    public async Task Handle(CompleteCurrentUserOnboardingCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);
        await repository.UpdateAsync(userId, user => user.CompleteOnboarding(), cancellationToken);
    }
}

public sealed class GetCurrentUserQueryHandler(IUserRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse?>
{
    public async Task<CurrentUserResponse?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var user = await UserLookup.RequireExistsAsync(repository, userId, cancellationToken);
        return new CurrentUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Nickname,
            user.Language,
            user.Theme,
            user.IsActive,
            user.HasCompletedOnboarding,
            user.IsEmailConfirmed,
            user.HasProfile);
    }
}

/// <summary>Preserves the exact "not found" message <c>LevelUpData.FindUser</c> used to throw.</summary>
internal static class UserLookup
{
    public static async Task<User> RequireExistsAsync(
        IUserRepository repository,
        Guid userId,
        CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidDomainStateException($"User '{userId}' was not found.");
}
