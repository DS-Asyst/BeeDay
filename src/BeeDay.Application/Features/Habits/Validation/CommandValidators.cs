using FluentValidation;
using LevelUp.Application.Features.Habits.Commands;

namespace LevelUp.Application.Features.Habits.Validation;

public sealed class CreateHabitCommandValidator : AbstractValidator<CreateHabitCommand>
{
    public CreateHabitCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveHabitRequestValidator());
}

public sealed class UpdateHabitCommandValidator : AbstractValidator<UpdateHabitCommand>
{
    public UpdateHabitCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveHabitRequestValidator());
}
