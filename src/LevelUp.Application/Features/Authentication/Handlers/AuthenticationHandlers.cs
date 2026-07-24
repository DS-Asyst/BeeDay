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

            data.SetCurrentUser(user.Id);
            user.RegisterLogin();
            response = new AuthenticatedUserResponse(
                user.Id,
                user.Name,
                user.Email,
                data.Characters.Any(character => character.UserId == user.Id),
                user.HasCompletedOnboarding);
        }, cancellationToken);

        return response!;
    }
}

public sealed class SelectAuthenticatedUserCommandHandler(ILevelUpRepository repository)
    : IRequestHandler<SelectAuthenticatedUserCommand>
{
    public Task Handle(SelectAuthenticatedUserCommand command, CancellationToken cancellationToken) =>
        repository.UpdateAsync(data => data.SetCurrentUser(command.UserId), cancellationToken);
}
