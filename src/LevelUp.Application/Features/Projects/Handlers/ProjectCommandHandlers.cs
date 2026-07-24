using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Projects.Commands;
using LevelUp.Domain.Entities;
using MediatR;

namespace LevelUp.Application.Features.Projects.Handlers;

public sealed class CreateProjectCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<CreateProjectCommand>
{
    public Task Handle(CreateProjectCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var request = command.Request;
            var project = Project.Create(request.Name, request.Description, request.Color, request.ExpectedDate);
            project.SetArchived(request.Archived);
            data.AddProject(project);
        }, cancellationToken);
}

public sealed class UpdateProjectCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<UpdateProjectCommand>
{
    public Task Handle(UpdateProjectCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var request = command.Request;
            var project = data.FindProject(command.Id);
            project.Update(request.Name, request.Description, request.Color, request.ExpectedDate);
            project.SetArchived(request.Archived);
        }, cancellationToken);
}

public sealed class DeleteProjectCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<DeleteProjectCommand>
{
    public Task Handle(DeleteProjectCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data => data.Projects.Remove(data.FindProject(command.Id)), cancellationToken);
}
