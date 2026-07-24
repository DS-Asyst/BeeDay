using FluentValidation;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Users.Validation;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(UserName.MaximumLength);
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(EmailAddress.MaximumLength);
        RuleFor(command => command.Request.Password)
            .MinimumLength(PasswordPolicy.MinimumLength)
            .MaximumLength(PasswordPolicy.MaximumLength)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .When(command => !string.IsNullOrWhiteSpace(command.Request.Password));
    }
}

public sealed class UpdateCurrentUserAccountCommandValidator : AbstractValidator<UpdateCurrentUserAccountCommand>
{
    public UpdateCurrentUserAccountCommandValidator()
    {
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(UserName.MaximumLength);
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(EmailAddress.MaximumLength);
    }
}

public sealed class UpdateCurrentUserPreferencesCommandValidator : AbstractValidator<UpdateCurrentUserPreferencesCommand>
{
    public UpdateCurrentUserPreferencesCommandValidator()
    {
        RuleFor(command => command.Request.Language).IsInEnum();
        RuleFor(command => command.Request.Theme).IsInEnum();
    }
}

public sealed class ChangeCurrentUserPasswordCommandValidator : AbstractValidator<ChangeCurrentUserPasswordCommand>
{
    public ChangeCurrentUserPasswordCommandValidator()
    {
        RuleFor(command => command.Request.CurrentPassword)
            .NotEmpty()
            .MaximumLength(PasswordPolicy.MaximumLength);

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
