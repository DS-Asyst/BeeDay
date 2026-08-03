using BeeDay.Application.Features.Projects.Requests;
using BeeDay.Domain.ValueObjects;
using FluentValidation;

namespace BeeDay.Application.Features.Projects.Validation;

public sealed class SaveProjectRequestValidator : AbstractValidator<SaveProjectRequest>
{
    public SaveProjectRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(ActivityTitle.MaximumLength)
            .WithMessage($"Name cannot exceed {ActivityTitle.MaximumLength} characters.");
        RuleFor(request => request.Description)
            .MaximumLength(ActivityDescription.MaximumLength)
            .WithMessage($"Description cannot exceed {ActivityDescription.MaximumLength} characters.");
        RuleFor(request => request.Color)
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must use the #RRGGBB format.");

        RuleFor(request => request.Attribute)
            .Must(attribute => !attribute.HasValue || Enum.IsDefined(attribute.Value))
            .WithMessage("Attribute is invalid.");
    }
}
