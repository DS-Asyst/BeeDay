using FluentValidation;
using LevelUp.Application.Features.Authentication.Commands;

namespace LevelUp.Application.Features.Authentication.Validation;

public sealed class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserCommandValidator()
    {
        RuleFor(command => command.Request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Request.Password)
            .NotEmpty();
    }
}
