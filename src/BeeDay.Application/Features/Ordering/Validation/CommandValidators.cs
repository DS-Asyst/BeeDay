using FluentValidation;
using LevelUp.Application.Features.Ordering.Commands;

namespace LevelUp.Application.Features.Ordering.Validation;

public sealed class ReorderActivitiesCommandValidator : AbstractValidator<ReorderActivitiesCommand>
{
    public ReorderActivitiesCommandValidator() => RuleFor(command => command.Request).SetValidator(new ReorderActivitiesRequestValidator());
}
