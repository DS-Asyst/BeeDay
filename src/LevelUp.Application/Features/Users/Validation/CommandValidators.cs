using FluentValidation;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Users.Validation;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(UserName.MaximumLength);
        RuleFor(command => command.Request.Email).NotEmpty().EmailAddress().MaximumLength(EmailAddress.MaximumLength);
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
