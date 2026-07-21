using LevelUp.Application.Features.Tasks.Requests;

namespace LevelUp.Application.Features.Tasks.Contracts;

public interface ITaskService
{
    public Task AddAsync(SaveTaskRequest request, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Guid id, SaveTaskRequest request, CancellationToken cancellationToken = default);
    public Task ToggleAsync(Guid id, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
