using BeeDay.Application.Features.Ordering.Requests;
using FluentValidation;

namespace BeeDay.Application.Features.Ordering.Validation;

public sealed class ReorderActivitiesRequestValidator : AbstractValidator<ReorderActivitiesRequest>
{
    public ReorderActivitiesRequestValidator()
    {
        RuleFor(request => request.Collection).IsInEnum();
        RuleFor(request => request.OrderedIds)
            .NotNull().WithMessage("The ordered identifier collection is required.")
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty))
            .WithMessage("Activity identifiers cannot be empty.")
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("The reorder request cannot contain duplicate identifiers.");
    }
}
