using BeeDay.Application.Features.Tasks.Commands;
using FluentValidation;

namespace BeeDay.Application.Features.Tasks.Validation;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveTaskRequestValidator());
}

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveTaskRequestValidator());
}
