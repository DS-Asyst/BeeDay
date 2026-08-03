using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Identity.Commands;
using FluentValidation;

namespace BeeDay.Application.Features.Identity.Validation;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator() => RuleFor(command => command.Request.Token).NotEmpty();
}

public sealed class ResendEmailConfirmationCommandValidator : AbstractValidator<ResendEmailConfirmationCommand>
{
    public ResendEmailConfirmationCommandValidator() =>
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress();
}

public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator() =>
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress();
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Request.Token).NotEmpty();
        RuleFor(command => command.Request.NewPassword)
            .NotEmpty()
            .MinimumLength(PasswordPolicy.MinimumLength)
            .MaximumLength(PasswordPolicy.MaximumLength)
            .Matches("[A-Za-z]").WithMessage("New password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("New password must contain at least one number.");
        RuleFor(command => command.Request.ConfirmNewPassword)
            .Equal(command => command.Request.NewPassword)
            .WithMessage("Password confirmation must match the new password.");
    }
}
