using FluentValidation;
using LevelUp.Application.Features.Projects.Commands;

namespace LevelUp.Application.Features.Projects.Validation;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveProjectRequestValidator());
}

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator() => RuleFor(command => command.Request).SetValidator(new SaveProjectRequestValidator());
}
