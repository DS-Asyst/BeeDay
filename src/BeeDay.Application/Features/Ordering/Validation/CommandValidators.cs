using BeeDay.Application.Features.Ordering.Commands;
using FluentValidation;

namespace BeeDay.Application.Features.Ordering.Validation;

public sealed class ReorderActivitiesCommandValidator : AbstractValidator<ReorderActivitiesCommand>
{
    public ReorderActivitiesCommandValidator() => RuleFor(command => command.Request).SetValidator(new ReorderActivitiesRequestValidator());
}
