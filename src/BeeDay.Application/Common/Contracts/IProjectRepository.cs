using BeeDay.Domain.Entities;

namespace BeeDay.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>Project</c> Aggregate, which owns <c>Todo</c> as a child entity.
/// There is deliberately no <c>ITodoRepository</c> — the Aggregate Map (05-domain-aggregate-map.md
/// §2.5) confirmed Todo's lifecycle is structurally bound to its owning Project (Project deletion
/// removes its Todos with no separate cleanup step), so a Todo is only ever reachable through this
/// port. <see cref="GetByTodoIdAsync"/> exists because use cases address a Todo by its own id without
/// already knowing which Project owns it.
/// </summary>
public interface IProjectRepository
{
    public Task<Project?> GetAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Project>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task<Project?> GetByTodoIdAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default);

    public Task AddAsync(Project project, CancellationToken cancellationToken = default);

    public Task RemoveAsync(Project project, CancellationToken cancellationToken = default);

    public Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedProjectIds,
        CancellationToken cancellationToken = default);

    public Task ReorderTodosAsync(
        Guid userId,
        Guid projectId,
        IReadOnlyList<Guid> orderedTodoIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the Project tracked, applies <paramref name="mutation"/>, and persists the result — see
    /// <see cref="IUserRepository.UpdateAsync"/> for why this shape, not a disconnected "Save".
    /// </summary>
    public Task UpdateAsync(
        Guid userId,
        Guid projectId,
        Action<Project> mutation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds <paramref name="todo"/> to an existing, already-persisted Project — <see cref="AddAsync"/>
    /// only covers a brand-new Project. Todo stays reachable exclusively through this port.
    /// </summary>
    public Task AddTodoAsync(
        Guid userId,
        Guid projectId,
        Todo todo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the Todo tracked by its own id, applies <paramref name="mutation"/>, and persists the
    /// result — see <see cref="IUserRepository.UpdateAsync"/> for why this shape.
    /// </summary>
    public Task UpdateTodoAsync(
        Guid userId,
        Guid todoId,
        Action<Todo> mutation,
        CancellationToken cancellationToken = default);

    public Task RemoveTodoAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reassigns a Todo from its current Project to <paramref name="destinationProjectId"/> — approved
    /// in docs/history/persistence-contracts.md §10, implemented here.
    /// </summary>
    public Task MoveTodoAsync(
        Guid userId,
        Guid todoId,
        Guid destinationProjectId,
        CancellationToken cancellationToken = default);
}
