using BeeDay.Application.Features.Todos.Requests;
using BeeDay.Domain.ValueObjects;
using FluentValidation;

namespace BeeDay.Application.Features.Todos.Validation;

public sealed class SaveTodoRequestValidator : AbstractValidator<SaveTodoRequest>
{
    public SaveTodoRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty().WithMessage("Project is required.");
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(ActivityTitle.MaximumLength)
            .WithMessage($"Title cannot exceed {ActivityTitle.MaximumLength} characters.");
        RuleFor(request => request.Description)
            .MaximumLength(ActivityDescription.MaximumLength)
            .WithMessage($"Description cannot exceed {ActivityDescription.MaximumLength} characters.");

        RuleFor(request => request.Attribute)
            .Must(attribute => !attribute.HasValue || Enum.IsDefined(attribute.Value))
            .WithMessage("Attribute is invalid.");
    }
}
