using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Identity;
using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Responses;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.ValueObjects;
using MediatR;

namespace BeeDay.Application.Features.Users.Handlers;

public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    IEmailConfirmationIssuer confirmationIssuer,
    IEmailSender emailSender,
    IIdentityRequestThrottle throttle) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Reuses the same per-email throttle already protecting resend-confirmation/forgot-password
        // (Epic 26, Sprint 26.7) — stops a rapid double-submit of the same address from ever issuing
        // two confirmation emails for one account. Does not, and cannot, limit registration volume
        // across distinct email addresses — that is a different abuse vector (mass registration with
        // many fake/victim addresses), deliberately not addressed here; see
        // docs/infrastructure/06-transactional-email.md §14 for why a new rate limiter for that case
        // was not built in this sprint.
        if (!throttle.TryAcquire("account-creation", command.Request.Email.Trim(), TimeSpan.FromSeconds(60), out var retryAfter))
        {
            throw new InvalidDomainStateException($"Please wait {Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))} seconds before trying again.");
        }

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
    IEmailSender emailSender,
    IIdentityRequestThrottle throttle) : IRequestHandler<CreateAccountCommand, Guid>
{
    public async Task<Guid> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        // See CreateUserCommandHandler above for the rationale and its documented limits.
        if (!throttle.TryAcquire("account-creation", command.Request.Email.Trim(), TimeSpan.FromSeconds(60), out var retryAfter))
        {
            throw new InvalidDomainStateException($"Please wait {Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))} seconds before trying again.");
        }

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

/// <summary>
/// EPIC 30 Sprint 30.11 / BD30-F044: an email change is treated as security-sensitive, exactly like a
/// password change (<see cref="ChangeCurrentUserPasswordCommandHandler"/>) — the current password must
/// be verified before it is allowed, otherwise a hijacked session (stolen cookie, XSS) alone would be
/// enough to repoint Email and, from there, take over the account via forgot-password. A Name-only
/// save never touches the password check. <see cref="User.UpdateAccount"/> itself resets
/// <c>IsEmailConfirmed</c> on an actual email change; this handler issues and sends the fresh
/// confirmation email the same way <see cref="CreateAccountCommandHandler"/> does at registration.
/// </summary>
public sealed class UpdateCurrentUserAccountCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    IEmailConfirmationIssuer confirmationIssuer,
    IEmailSender emailSender,
    IClock clock,
    ICurrentUserContext currentUser) : IRequestHandler<UpdateCurrentUserAccountCommand>
{
    public async Task Handle(UpdateCurrentUserAccountCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var user = await UserLookup.RequireExistsAsync(unitOfWork.Users, userId, cancellationToken);

        var request = command.Request;
        if (await unitOfWork.Users.IsEmailInUseAsync(request.Email.Trim(), userId, cancellationToken))
        {
            throw new InvalidDomainStateException($"Email '{request.Email}' is already registered.");
        }

        var emailChanged = !string.Equals(user.Email, EmailAddress.Create(request.Email).Value, StringComparison.Ordinal);
        if (emailChanged &&
            (string.IsNullOrWhiteSpace(user.PasswordHash) || !passwordService.Verify(request.CurrentPassword, user.PasswordHash)))
        {
            throw new InvalidDomainStateException("The current password is incorrect.");
        }

        EmailMessage? confirmationEmail = null;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Users.UpdateAsync(userId, u => u.UpdateAccount(request.Name, request.Email), cancellationToken);

            if (emailChanged)
            {
                var now = clock.UtcNow;
                await unitOfWork.UserTokens.RevokeActiveAsync(userId, UserTokenType.EmailConfirmation, now, cancellationToken);

                var updatedUser = await UserLookup.RequireExistsAsync(unitOfWork.Users, userId, cancellationToken);
                var (token, message) = confirmationIssuer.Issue(updatedUser);
                await unitOfWork.UserTokens.AddAsync(token, cancellationToken);
                confirmationEmail = message;
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }

        if (confirmationEmail is not null)
        {
            await emailSender.SendAsync(confirmationEmail, cancellationToken);
        }
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
