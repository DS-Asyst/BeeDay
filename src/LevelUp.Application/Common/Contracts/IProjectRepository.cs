using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

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
}
