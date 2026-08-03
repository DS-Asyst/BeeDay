using BeeDay.Application.Features.Habits.Commands;
using FluentValidation;

namespace BeeDay.Application.Features.Habits.Validation;

public sealed class CreateHabitCommandValidator : AbstractValidator<CreateHabitCommand>
{
    public CreateHabitCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveHabitRequestValidator());
}

public sealed class UpdateHabitCommandValidator : AbstractValidator<UpdateHabitCommand>
{
    public UpdateHabitCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveHabitRequestValidator());
}
