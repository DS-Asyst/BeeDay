using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfHabitRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : IHabitRepository
{
    public async Task<Habit?> GetAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Habits
            .AsNoTracking()
            .FirstOrDefaultAsync(habit => habit.UserId == userId && habit.Id == habitId, cancellationToken);
    }

    public async Task<IReadOnlyList<Habit>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Habits
            .AsNoTracking()
            .Where(habit => habit.UserId == userId)
            .OrderBy(habit => EF.Property<int>(habit, "Position"))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Position has no database default (01-relational-model.md §5.2 defines it as an
        // Infrastructure-only ordinal, not a Domain concept) — the repository owns assigning the next
        // free slot, scoped by UserId, or every new Habit would insert with Position = 0.
        var maxPosition = await context.Habits
            .Where(existing => existing.UserId == habit.UserId)
            .Select(existing => (int?)EF.Property<int>(existing, "Position"))
            .MaxAsync(cancellationToken);

        context.Habits.Add(habit);
        context.Entry(habit).Property("Position").CurrentValue = (maxPosition ?? -1) + 1;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // habit was materialized by a different, already-disposed context (e.g. a prior GetAsync) — its
        // RowVersion (shadow, never exposed to Domain) is unknown to this instance. Re-fetching by Id
        // tracks the current row, including its real RowVersion, so the DELETE's concurrency check is
        // genuine instead of a guaranteed mismatch against a default value.
        var tracked = await context.Habits.SingleAsync(existing => existing.Id == habit.Id, cancellationToken);
        context.Habits.Remove(tracked);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedHabitIds,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var habitsById = await context.Habits
            .Where(habit => habit.UserId == userId)
            .ToDictionaryAsync(habit => habit.Id, cancellationToken);

        for (var index = 0; index < orderedHabitIds.Count; index++)
        {
            if (habitsById.TryGetValue(orderedHabitIds[index], out var habit))
            {
                context.Entry(habit).Property("Position").CurrentValue = index;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
