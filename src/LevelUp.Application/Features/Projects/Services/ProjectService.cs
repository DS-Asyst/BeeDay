using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Services;
using LevelUp.Application.Features.Projects.Contracts;
using LevelUp.Application.Features.Projects.Requests;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Features.Projects.Services;

public sealed class ProjectService(ILevelUpRepository repository)
    : ApplicationService(repository), IProjectService
{
    public Task AddAsync(SaveProjectRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => data.AddProject(Project.Create(
            request.Title,
            request.Description,
            request.Status)), cancellationToken);

    public Task UpdateAsync(Guid id, SaveProjectRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Projects, id).Update(
            request.Title,
            request.Description,
            request.Status), cancellationToken);

    public Task ToggleAsync(Guid id, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Projects, id).ToggleStatus(), cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteAsync(data => data.Projects, id, cancellationToken);
}
