using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Projects.Commands;
using LevelUp.Domain.Entities;
using MediatR;

namespace LevelUp.Application.Features.Projects.Handlers;

public sealed class CreateProjectCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<CreateProjectCommand>
{
    public Task Handle(CreateProjectCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var request = command.Request;
        var project = Project.Create(request.Name, request.Description, request.Color, request.ExpectedDate, request.Attribute);
        project.SetArchived(request.Archived);
        data.AddProject(CurrentUserGuard.RequireUserId(data, currentUser), project);
    }, cancellationToken);
}

public sealed class UpdateProjectCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<UpdateProjectCommand>
{
    public Task Handle(UpdateProjectCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var request = command.Request;
        var project = data.FindProject(CurrentUserGuard.RequireUserId(data, currentUser), command.Id);
        project.Update(request.Name, request.Description, request.Color, request.ExpectedDate, request.Attribute);
        project.SetArchived(request.Archived);
    }, cancellationToken);
}

public sealed class DeleteProjectCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<DeleteProjectCommand>
{
    public Task Handle(DeleteProjectCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var project = data.FindProject(CurrentUserGuard.RequireUserId(data, currentUser), command.Id);
        data.Projects.Remove(project);
    }, cancellationToken);
}
