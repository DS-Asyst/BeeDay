using LevelUp.Application.Features.Projects.Requests;

namespace LevelUp.Application.Features.Projects.Contracts;

public interface IProjectService
{
    public Task AddAsync(SaveProjectRequest request, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Guid id, SaveProjectRequest request, CancellationToken cancellationToken = default);
    public Task ToggleAsync(Guid id, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
