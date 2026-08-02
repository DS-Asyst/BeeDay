using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Contracts;

/// <summary>
/// Persistence boundary for the <c>Habit</c> Aggregate. Kept separate from
/// <see cref="IRecurringTaskRepository"/> despite the shared <c>Activity</c> base class — Habit and
/// RecurringTask are distinct Aggregate Roots with independent identity and lifecycle (see
/// docs/architecture/05-domain-aggregate-map.md §2.3/§2.4), so a single combined port would tie two
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
}
