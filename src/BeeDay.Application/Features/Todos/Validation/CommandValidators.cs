using FluentValidation;
using LevelUp.Application.Features.Todos.Commands;

namespace LevelUp.Application.Features.Todos.Validation;

public sealed class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveTodoRequestValidator());
}

public sealed class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveTodoRequestValidator());
}
