using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Services;
using LevelUp.Application.Features.Todos.Contracts;
using LevelUp.Application.Features.Todos.Requests;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Features.Todos.Services;

public sealed class TodoService(ILevelUpRepository repository)
    : ApplicationService(repository), ITodoService
{
    public Task AddAsync(SaveTodoRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => data.AddTodo(Todo.Create(
            request.Title,
            request.Description,
            request.DueDate)), cancellationToken);

    public Task UpdateAsync(Guid id, SaveTodoRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Todos, id).Update(
            request.Title,
            request.Description,
            request.DueDate), cancellationToken);

    public Task ToggleAsync(Guid id, CancellationToken cancellationToken = default) =>
        MutateAsync(data => Find(data.Todos, id).ToggleCompletion(), cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteAsync(data => data.Todos, id, cancellationToken);
}
