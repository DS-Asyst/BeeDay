using FluentValidation;
using LevelUp.Application.Features.Projects.Requests;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Projects.Validation;

public sealed class SaveProjectRequestValidator : AbstractValidator<SaveProjectRequest>
{
    public SaveProjectRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(ActivityTitle.MaximumLength)
            .WithMessage($"Title cannot exceed {ActivityTitle.MaximumLength} characters.");
        RuleFor(request => request.Description)
            .MaximumLength(ActivityDescription.MaximumLength)
            .WithMessage($"Description cannot exceed {ActivityDescription.MaximumLength} characters.");
        RuleFor(request => request.Status).IsInEnum();
    }
}
