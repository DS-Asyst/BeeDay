using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Projects.Commands;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Exceptions;
using MediatR;

namespace BeeDay.Application.Features.Projects.Handlers;

public sealed class CreateProjectCommandHandler(IProjectRepository repository, ICurrentUserContext currentUser) : IRequestHandler<CreateProjectCommand>
{
    public Task Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;
        var project = Project.Create(request.Name, request.Description, request.Color, request.ExpectedDate, request.Attribute);
        project.SetArchived(request.Archived);
        project.AssignOwner(userId);
        return repository.AddAsync(project, cancellationToken);
    }
}

public sealed class UpdateProjectCommandHandler(IProjectRepository repository, ICurrentUserContext currentUser) : IRequestHandler<UpdateProjectCommand>
{
    public async Task Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await ProjectLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);

        var request = command.Request;
        await repository.UpdateAsync(userId, command.Id, project =>
        {
            project.Update(request.Name, request.Description, request.Color, request.ExpectedDate, request.Attribute);
            project.SetArchived(request.Archived);
        }, cancellationToken);
    }
}

public sealed class DeleteProjectCommandHandler(IProjectRepository repository, ICurrentUserContext currentUser) : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var project = await ProjectLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);
        await repository.RemoveAsync(project, cancellationToken);
    }
}

/// <summary>Preserves the exact "not found" message <c>LevelUpData.FindProject(userId, projectId)</c> used to throw.</summary>
internal static class ProjectLookup
{
    public static async Task<Project> RequireExistsAsync(
        IProjectRepository repository,
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(userId, projectId, cancellationToken)
            ?? throw new InvalidDomainStateException($"Project '{projectId}' was not found for the authenticated User.");
}
