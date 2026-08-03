using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Authentication.Commands;
using LevelUp.Application.Features.Authentication.Responses;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Authentication.Handlers;

public sealed class AuthenticateUserCommandHandler(
    IUserRepository repository,
    IPasswordService passwordService)
    : IRequestHandler<AuthenticateUserCommand, AuthenticatedUserResponse>
{
    public async Task<AuthenticatedUserResponse> Handle(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim();
        var user = await repository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordService.Verify(command.Request.Password, user.PasswordHash))
        {
            throw new InvalidDomainStateException("Invalid email or password.");
        }

        if (!user.IsEmailConfirmed)
        {
            throw new InvalidDomainStateException("Invalid email or password.");
        }

        AuthenticatedUserResponse? response = null;
        await repository.UpdateAsync(user.Id, tracked =>
        {
            if (passwordService.NeedsRehash(tracked.PasswordHash))
            {
                // A transparent hash-format upgrade, not a security-relevant password change:
                // must not invalidate this or any other session.
                tracked.SetPasswordHash(passwordService.Hash(command.Request.Password));
            }

            tracked.RegisterLogin();
            response = new AuthenticatedUserResponse(
                tracked.Id,
                tracked.Name,
                tracked.Email,
                tracked.HasProfile,
                tracked.HasCompletedOnboarding,
                tracked.SessionVersion);
        }, cancellationToken);

        return response!;
    }
}
