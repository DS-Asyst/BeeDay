using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfRecurringTaskRepository : EfRepositoryBase, IRecurringTaskRepository
{
    public EfRecurringTaskRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfRecurringTaskRepository(LevelUpDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<RecurringTask?> GetAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.RecurringTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(task => task.UserId == userId && task.Id == taskId, cancellationToken);
    }

    public async Task<IReadOnlyList<RecurringTask>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.RecurringTasks
            .AsNoTracking()
            .Where(task => task.UserId == userId)
            .OrderBy(task => EF.Property<int>(task, "Position"))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RecurringTask task, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfHabitRepository.AddAsync — Position has no database default, the repository assigns
        // the next free slot scoped by UserId.
        var maxPosition = await context.RecurringTasks
            .Where(existing => existing.UserId == task.UserId)
            .Select(existing => (int?)EF.Property<int>(existing, "Position"))
            .MaxAsync(cancellationToken);

        context.RecurringTasks.Add(task);
        context.Entry(task).Property("Position").CurrentValue = (maxPosition ?? -1) + 1;

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task RemoveAsync(RecurringTask task, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfHabitRepository.RemoveAsync for why this re-fetches by Id instead of attaching `task`.
        var tracked = await context.RecurringTasks.SingleAsync(existing => existing.Id == task.Id, cancellationToken);
        context.RecurringTasks.Remove(tracked);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task ReorderAsync(
        Guid userId,
        IReadOnlyList<Guid> orderedTaskIds,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        var tasksById = await context.RecurringTasks
            .Where(task => task.UserId == userId)
            .ToDictionaryAsync(task => task.Id, cancellationToken);

        for (var index = 0; index < orderedTaskIds.Count; index++)
        {
            if (tasksById.TryGetValue(orderedTaskIds[index], out var task))
            {
                context.Entry(task).Property("Position").CurrentValue = index;
            }
        }

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid userId,
        Guid taskId,
        Action<RecurringTask> mutation,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfUserRepository.UpdateAsync for why tracked-and-mutated-within-this-call, not a
        // disconnected Save.
        var task = await context.RecurringTasks
            .SingleAsync(existing => existing.UserId == userId && existing.Id == taskId, cancellationToken);
        mutation(task);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }
}
