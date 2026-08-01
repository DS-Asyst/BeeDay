using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Identity;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Identity.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Identity.Handlers;

public static class IdentityTokenLifetimes
{
    public static readonly TimeSpan EmailConfirmation = TimeSpan.FromHours(24);
    public static readonly TimeSpan PasswordReset = TimeSpan.FromHours(1);
}

public sealed class ConfirmEmailCommandHandler(
    ILevelUpRepository repository,
    IUserTokenService tokenService,
    IClock clock) : IRequestHandler<ConfirmEmailCommand>
{
    public Task Handle(ConfirmEmailCommand command, CancellationToken cancellationToken) =>
        repository.UpdateAsync(data =>
        {
            var now = clock.UtcNow;
            var token = FindToken(data, tokenService.HashToken(command.Request.Token), UserTokenType.EmailConfirmation);
            token.EnsureCanBeUsed(UserTokenType.EmailConfirmation, now);
            data.FindUser(token.UserId).ConfirmEmail(now);
            token.MarkAsUsed(UserTokenType.EmailConfirmation, now);
        }, cancellationToken);

    private static UserToken FindToken(LevelUpData data, string tokenHash, UserTokenType type) =>
        data.UserTokens.FirstOrDefault(candidate =>
            candidate.Type == type && string.Equals(candidate.TokenHash, tokenHash, StringComparison.Ordinal))
        ?? throw new InvalidDomainStateException("The email confirmation token is invalid or expired.");
}

public sealed class ResendEmailConfirmationCommandHandler(
    ILevelUpRepository repository,
    IUserTokenService tokenService,
    IIdentityEmailComposer emailComposer,
    IEmailSender emailSender,
    IIdentityRequestThrottle throttle,
    IClock clock) : IRequestHandler<ResendEmailConfirmationCommand>
{
    public async Task Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim();
        if (!throttle.TryAcquire("email-confirmation", email, TimeSpan.FromSeconds(60), out var retryAfter))
        {
            throw new InvalidDomainStateException($"Please wait {Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))} seconds before requesting another email.");
        }

        EmailMessage? message = null;
        await repository.UpdateAsync(data =>
        {
            var user = FindUserByEmail(data, email);
            if (user is null || !user.IsActive || user.IsEmailConfirmed)
            {
                return;
            }

            var now = clock.UtcNow;
            data.RevokeActiveUserTokens(user.Id, UserTokenType.EmailConfirmation, now);
            var rawToken = tokenService.GenerateToken();
            data.AddUserToken(UserToken.Create(
                user.Id,
                UserTokenType.EmailConfirmation,
                tokenService.HashToken(rawToken),
                now,
                now.Add(IdentityTokenLifetimes.EmailConfirmation)));
            message = emailComposer.ComposeEmailConfirmation(user.Email, user.Name, rawToken);
        }, cancellationToken);

        if (message is not null)
        {
            await emailSender.SendAsync(message, cancellationToken);
        }
    }

    private static User? FindUserByEmail(LevelUpData data, string email) =>
        data.Users.FirstOrDefault(candidate =>
            string.Equals(candidate.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
}

public sealed class RequestPasswordResetCommandHandler(
    ILevelUpRepository repository,
    IUserTokenService tokenService,
    IIdentityEmailComposer emailComposer,
    IEmailSender emailSender,
    IIdentityRequestThrottle throttle,
    IClock clock) : IRequestHandler<RequestPasswordResetCommand>
{
    public async Task Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim();
        if (!throttle.TryAcquire("password-reset", email, TimeSpan.FromSeconds(60), out _))
        {
            return;
        }

        EmailMessage? message = null;
        await repository.UpdateAsync(data =>
        {
            var user = data.Users.FirstOrDefault(candidate =>
                string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));
            if (user is null || !user.IsActive || !user.IsEmailConfirmed)
            {
                return;
            }

            var now = clock.UtcNow;
            data.RevokeActiveUserTokens(user.Id, UserTokenType.PasswordReset, now);
            var rawToken = tokenService.GenerateToken();
            data.AddUserToken(UserToken.Create(
                user.Id,
                UserTokenType.PasswordReset,
                tokenService.HashToken(rawToken),
                now,
                now.Add(IdentityTokenLifetimes.PasswordReset)));
            message = emailComposer.ComposePasswordReset(user.Email, user.Name, rawToken);
        }, cancellationToken);

        if (message is not null)
        {
            await emailSender.SendAsync(message, cancellationToken);
        }
    }
}

public sealed class ResetPasswordCommandHandler(
    ILevelUpRepository repository,
    IUserTokenService tokenService,
    IPasswordService passwordService,
    IClock clock) : IRequestHandler<ResetPasswordCommand>
{
    public Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken) =>
        repository.UpdateAsync(data =>
        {
            var now = clock.UtcNow;
            var tokenHash = tokenService.HashToken(command.Request.Token);
            var token = data.UserTokens.FirstOrDefault(candidate =>
                candidate.Type == UserTokenType.PasswordReset &&
                string.Equals(candidate.TokenHash, tokenHash, StringComparison.Ordinal))
                ?? throw new InvalidDomainStateException("The password reset token is invalid or expired.");

            token.EnsureCanBeUsed(UserTokenType.PasswordReset, now);
            var user = data.FindUser(token.UserId);
            user.SetPasswordHash(passwordService.Hash(command.Request.NewPassword));
            user.InvalidateSessions();
            token.MarkAsUsed(UserTokenType.PasswordReset, now);
            data.RevokeActiveUserTokens(user.Id, UserTokenType.PasswordReset, now);
        }, cancellationToken);
}
