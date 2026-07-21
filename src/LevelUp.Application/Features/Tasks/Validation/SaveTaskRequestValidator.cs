using FluentValidation;
using LevelUp.Application.Features.Tasks.Requests;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Tasks.Validation;

public sealed class SaveTaskRequestValidator : AbstractValidator<SaveTaskRequest>
{
    public SaveTaskRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(ActivityTitle.MaximumLength)
            .WithMessage($"Title cannot exceed {ActivityTitle.MaximumLength} characters.");
        RuleFor(request => request.Description)
            .MaximumLength(ActivityDescription.MaximumLength)
            .WithMessage($"Description cannot exceed {ActivityDescription.MaximumLength} characters.");
        RuleFor(request => request.Repeat).IsInEnum();
    }
}
