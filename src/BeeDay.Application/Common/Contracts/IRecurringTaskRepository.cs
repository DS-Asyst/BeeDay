using BeeDay.Domain.Entities;

namespace BeeDay.Application.Common.Contracts;

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

    /// <summary>
    /// Loads the RecurringTask tracked, applies <paramref name="mutation"/>, and persists the result —
    /// see <see cref="IUserRepository.UpdateAsync"/> for why this shape, not a disconnected "Save".
    /// </summary>
    public Task UpdateAsync(
        Guid userId,
        Guid taskId,
        Action<RecurringTask> mutation,
        CancellationToken cancellationToken = default);
}
