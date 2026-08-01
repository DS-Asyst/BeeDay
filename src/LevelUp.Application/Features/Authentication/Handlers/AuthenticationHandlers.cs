using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Authentication.Commands;
using LevelUp.Application.Features.Authentication.Responses;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Authentication.Handlers;

public sealed class AuthenticateUserCommandHandler(
    ILevelUpRepository repository,
    IPasswordService passwordService)
    : IRequestHandler<AuthenticateUserCommand, AuthenticatedUserResponse>
{
    public async Task<AuthenticatedUserResponse> Handle(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken)
    {
        AuthenticatedUserResponse? response = null;

        await repository.UpdateAsync(data =>
        {
            var email = command.Request.Email.Trim();
            var user = data.Users.FirstOrDefault(candidate =>
                string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));

            if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.PasswordHash) ||
                !passwordService.Verify(command.Request.Password, user.PasswordHash))
            {
                throw new InvalidDomainStateException("Invalid email or password.");
            }

            if (!user.IsEmailConfirmed)
            {
                throw new InvalidDomainStateException("Invalid email or password.");
            }

            if (passwordService.NeedsRehash(user.PasswordHash))
            {
                // A transparent hash-format upgrade, not a security-relevant password change:
                // must not invalidate this or any other session.
                user.SetPasswordHash(passwordService.Hash(command.Request.Password));
            }

            user.RegisterLogin();
            response = new AuthenticatedUserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.HasProfile,
                user.HasCompletedOnboarding,
                user.SessionVersion);
        }, cancellationToken);

        return response!;
    }
}
