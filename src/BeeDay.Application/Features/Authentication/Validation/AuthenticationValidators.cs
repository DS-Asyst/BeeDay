using BeeDay.Application.Features.Authentication.Commands;
using FluentValidation;

namespace BeeDay.Application.Features.Authentication.Validation;

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
