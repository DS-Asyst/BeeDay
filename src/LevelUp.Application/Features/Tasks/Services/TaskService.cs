using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Services;
using LevelUp.Application.Features.Tasks.Contracts;
using LevelUp.Application.Features.Tasks.Requests;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Features.Tasks.Services;

public sealed class TaskService(ILevelUpRepository repository)
    : ApplicationService(repository), ITaskService
{
    public Task AddAsync(SaveTaskRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => data.AddTask(RecurringTask.Create(
            request.Title,
            request.Description,
            request.Repeat)), cancellationToken);

    public Task UpdateAsync(Guid id, SaveTaskRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Tasks, id).Update(
            request.Title,
            request.Description,
            request.Repeat), cancellationToken);

    public Task ToggleAsync(Guid id, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Tasks, id).ToggleCompletion(), cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteAsync(data => data.Tasks, id, cancellationToken);
}
