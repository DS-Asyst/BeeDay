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
    IUnitOfWork unitOfWork,
    IUserTokenService tokenService,
    IClock clock) : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenService.HashToken(command.Request.Token);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var token = await unitOfWork.UserTokens.GetByHashAsync(tokenHash, UserTokenType.EmailConfirmation, cancellationToken)
                ?? throw new InvalidDomainStateException("The email confirmation token is invalid or expired.");
            token.EnsureCanBeUsed(UserTokenType.EmailConfirmation, now);

            await unitOfWork.Users.UpdateAsync(token.UserId, user => user.ConfirmEmail(now), cancellationToken);
            await unitOfWork.UserTokens.UpdateAsync(token.Id, t => t.MarkAsUsed(UserTokenType.EmailConfirmation, now), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}

public sealed class ResendEmailConfirmationCommandHandler(
    IUserRepository userRepository,
    IUserTokenRepository userTokenRepository,
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

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || user.IsEmailConfirmed)
        {
            return;
        }

        var now = clock.UtcNow;
        await userTokenRepository.RevokeActiveAsync(user.Id, UserTokenType.EmailConfirmation, now, cancellationToken);

        var rawToken = tokenService.GenerateToken();
        await userTokenRepository.AddAsync(
            UserToken.Create(
                user.Id,
                UserTokenType.EmailConfirmation,
                tokenService.HashToken(rawToken),
                now,
                now.Add(IdentityTokenLifetimes.EmailConfirmation)),
            cancellationToken);

        var message = emailComposer.ComposeEmailConfirmation(user.Email, user.Name, rawToken);
        await emailSender.SendAsync(message, cancellationToken);
    }
}

public sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IUserTokenRepository userTokenRepository,
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

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || !user.IsEmailConfirmed)
        {
            return;
        }

        var now = clock.UtcNow;
        await userTokenRepository.RevokeActiveAsync(user.Id, UserTokenType.PasswordReset, now, cancellationToken);

        var rawToken = tokenService.GenerateToken();
        await userTokenRepository.AddAsync(
            UserToken.Create(
                user.Id,
                UserTokenType.PasswordReset,
                tokenService.HashToken(rawToken),
                now,
                now.Add(IdentityTokenLifetimes.PasswordReset)),
            cancellationToken);

        var message = emailComposer.ComposePasswordReset(user.Email, user.Name, rawToken);
        await emailSender.SendAsync(message, cancellationToken);
    }
}

public sealed class ResetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUserTokenService tokenService,
    IPasswordService passwordService,
    IClock clock) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var tokenHash = tokenService.HashToken(command.Request.Token);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var token = await unitOfWork.UserTokens.GetByHashAsync(tokenHash, UserTokenType.PasswordReset, cancellationToken)
                ?? throw new InvalidDomainStateException("The password reset token is invalid or expired.");
            token.EnsureCanBeUsed(UserTokenType.PasswordReset, now);

            await unitOfWork.Users.UpdateAsync(token.UserId, user =>
            {
                user.SetPasswordHash(passwordService.Hash(command.Request.NewPassword));
                user.InvalidateSessions();
            }, cancellationToken);

            await unitOfWork.UserTokens.UpdateAsync(token.Id, t => t.MarkAsUsed(UserTokenType.PasswordReset, now), cancellationToken);
            await unitOfWork.UserTokens.RevokeActiveAsync(token.UserId, UserTokenType.PasswordReset, now, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}
