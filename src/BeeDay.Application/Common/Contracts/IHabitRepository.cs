using BeeDay.Domain.Entities;

namespace BeeDay.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>Habit</c> Aggregate. Kept separate from
/// <see cref="IRecurringTaskRepository"/> despite the shared <c>Activity</c> base class — Habit and
/// RecurringTask are distinct Aggregate Roots with independent identity and lifecycle (see
/// docs/history/domain-aggregate-map.md §2.3/§2.4), so a single combined port would tie two
/// unrelated aggregates to one contract.
/// </summary>
public interface IHabitRepository
{
    public Task<Habit?> GetAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Habit>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    public Task AddAsync(Habit habit, CancellationToken cancellationToken = default);

    public Task RemoveAsync(Habit habit, CancellationToken cancellationToken = default);

    public Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedHabitIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the Habit tracked, applies <paramref name="mutation"/>, and persists the result — see
    /// <see cref="IUserRepository.UpdateAsync"/> for why this shape, not a disconnected "Save".
    /// </summary>
    public Task UpdateAsync(
        Guid userId,
        Guid habitId,
        Action<Habit> mutation,
        CancellationToken cancellationToken = default);
}
