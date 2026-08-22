using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Users.Commands;
using BeeDay.Domain.ValueObjects;
using FluentValidation;

namespace BeeDay.Application.Features.Users.Validation;

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

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(command => command.Request.Name)
            .NotEmpty()
            .MaximumLength(UserName.MaximumLength);
        RuleFor(command => command.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(EmailAddress.MaximumLength);
        RuleFor(command => command.Request.Password)
            .NotEmpty()
            .MinimumLength(PasswordPolicy.MinimumLength)
            .MaximumLength(PasswordPolicy.MaximumLength)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        RuleFor(command => command.Request.Nickname)
            .NotEmpty()
            .MinimumLength(Nickname.MinimumLength)
            .MaximumLength(Nickname.MaximumLength)
            .Matches("^[A-Za-z0-9._-]+$");
    }
}

public sealed class CompleteUserProfileCommandValidator : AbstractValidator<CompleteUserProfileCommand>
{
    public CompleteUserProfileCommandValidator()
    {
        RuleFor(command => command.Request.FullName)
            .NotEmpty()
            .MaximumLength(UserName.MaximumLength)
            .OverridePropertyName("FullName");
        RuleFor(command => command.Request.Nickname)
            .NotEmpty()
            .MinimumLength(Nickname.MinimumLength)
            .MaximumLength(Nickname.MaximumLength)
            .Matches("^[A-Za-z0-9._-]+$")
            .OverridePropertyName("Nickname");
    }
}

public sealed class UpdateCurrentUserAvatarCommandValidator : AbstractValidator<UpdateCurrentUserAvatarCommand>
{
    private const int MaximumAvatarLength = 2048;

    public UpdateCurrentUserAvatarCommandValidator()
    {
        RuleFor(command => command.Request.Avatar)
            .MaximumLength(MaximumAvatarLength);
    }
}

public sealed class UpdateCurrentUserAccountCommandValidator : AbstractValidator<UpdateCurrentUserAccountCommand>
{
    public UpdateCurrentUserAccountCommandValidator()
    {
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(UserName.MaximumLength);
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(EmailAddress.MaximumLength);

        // Not NotEmpty(): only required when Email actually changes, a check
        // UpdateCurrentUserAccountCommandHandler makes (it needs the current user's stored Email to
        // know that), not something this validator can express on the request alone.
        RuleFor(command => command.Request.CurrentPassword).MaximumLength(PasswordPolicy.MaximumLength);
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
