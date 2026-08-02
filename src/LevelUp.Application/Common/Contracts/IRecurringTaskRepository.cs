using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>RecurringTask</c> Aggregate. Kept separate from
/// <see cref="IHabitRepository"/> — see the rationale on that interface.
/// </summary>
public interface IRecurringTaskRepository
{
    public Task<RecurringTask?> GetAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RecurringTask>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task AddAsync(RecurringTask task, CancellationToken cancellationToken = default);

    public Task RemoveAsync(RecurringTask task, CancellationToken cancellationToken = default);

    public Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedTaskIds,
        CancellationToken cancellationToken = default);
}
