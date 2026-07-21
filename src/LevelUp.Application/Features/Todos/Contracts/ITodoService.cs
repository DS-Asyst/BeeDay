using LevelUp.Application.Features.Todos.Requests;

namespace LevelUp.Application.Features.Todos.Contracts;

public interface ITodoService
{
    public Task AddAsync(SaveTodoRequest request, CancellationToken cancellationToken = default);
    public Task UpdateAsync(Guid id, SaveTodoRequest request, CancellationToken cancellationToken = default);
    public Task ToggleAsync(Guid id, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
