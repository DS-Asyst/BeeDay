using FluentValidation;
using LevelUp.Application.Features.Profiles.Commands;

namespace LevelUp.Application.Features.Profiles.Validation;

public sealed class SaveProfileCommandValidator : AbstractValidator<SaveProfileCommand>
{
    public SaveProfileCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveProfileRequestValidator());
}
