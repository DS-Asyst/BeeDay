using FluentValidation;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Characters.Commands;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Characters.Validation;


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
            .MinimumLength(CharacterNickname.MinimumLength)
            .MaximumLength(CharacterNickname.MaximumLength)
            .Matches("^[A-Za-z0-9._-]+$");
        RuleFor(command => command.Request.CharacterClass)
            .IsInEnum();
    }
}

public sealed class CreateCharacterCommandValidator : AbstractValidator<CreateCharacterCommand>
{
    public CreateCharacterCommandValidator()
    {
        RuleFor(command => command.Request.UserName)
            .NotEmpty()
            .MaximumLength(UserName.MaximumLength)
            .OverridePropertyName("UserName");
        RuleFor(command => command.Request.Nickname)
            .NotEmpty()
            .MinimumLength(CharacterNickname.MinimumLength)
            .MaximumLength(CharacterNickname.MaximumLength)
            .Matches("^[A-Za-z0-9._-]+$")
            .OverridePropertyName("Nickname");
        RuleFor(command => command.Request.CharacterClass)
            .IsInEnum()
            .OverridePropertyName("CharacterClass");
    }
}

public sealed class UpdateCurrentCharacterAvatarCommandValidator : AbstractValidator<UpdateCurrentCharacterAvatarCommand>
{
    private const int MaximumAvatarLength = 2048;

    public UpdateCurrentCharacterAvatarCommandValidator()
    {
        RuleFor(command => command.Request.Avatar)
            .MaximumLength(MaximumAvatarLength);
    }
}
