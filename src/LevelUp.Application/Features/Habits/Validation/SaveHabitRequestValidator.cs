using FluentValidation;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Habits.Validation;

public sealed class SaveHabitRequestValidator : AbstractValidator<SaveHabitRequest>
{
    public SaveHabitRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(ActivityTitle.MaximumLength)
            .WithMessage($"Title cannot exceed {ActivityTitle.MaximumLength} characters.");

        RuleFor(request => request.Description)
            .MaximumLength(ActivityDescription.MaximumLength)
            .WithMessage($"Description cannot exceed {ActivityDescription.MaximumLength} characters.");

        RuleFor(request => request.Direction).IsInEnum();
        RuleFor(request => request.Difficulty).IsInEnum();
        RuleFor(request => request.ResetCounter).IsInEnum();
    }
}
