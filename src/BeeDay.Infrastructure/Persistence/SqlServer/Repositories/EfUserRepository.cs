using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfUserRepository : EfRepositoryBase, IUserRepository
{
    public EfUserRepository(IDbContextFactory<BeeDayDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfUserRepository(BeeDayDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> IsEmailInUseAsync(
        string normalizedEmail,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Users
            .AsNoTracking()
            .Where(user => user.Email == normalizedEmail)
            .Where(user => excludingUserId == null || user.Id != excludingUserId)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> IsNicknameInUseAsync(
        string normalizedNickname,
        Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Users
            .AsNoTracking()
            .Where(user => user.Nickname == normalizedNickname)
            .Where(user => excludingUserId == null || user.Id != excludingUserId)
            .AnyAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        lease.Context.Users.Add(user);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task UpdateAsync(Guid userId, Action<User> mutation, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // Tracked (not AsNoTracking) and mutated within this same call, right before SaveChanges — the
        // RowVersion captured here is whatever is currently in the database, so a genuinely concurrent
        // write is still caught. A disconnected "SaveAsync(User, ct)" taking an already-mutated,
        // detached instance from a prior call cannot offer this guarantee: Domain never carries
        // RowVersion, so re-attaching it would either use a stale/default token (guaranteed false
        // conflict) or require re-reading the token right before saving (which stops detecting the
        // exact conflict RowVersion exists to catch — a write that happened between the original read
        // and this save).
        var user = await context.Users.SingleAsync(existing => existing.Id == userId, cancellationToken);

        // UserExperience.Entries is intentionally not an EF Core-mapped navigation (ExperienceEntry is
        // its own top-level entity related to User directly, not through UserExperience — see
        // ExperienceEntryConfiguration.cs) and is never included by the query above. Hydrated explicitly
        // here so ExperienceRewardService.Grant → User.TryAddExperience → UserExperience.TryAdd sees
        // real history instead of always comparing against an empty collection: without this, the
        // in-memory duplicate check could never catch a repeat grant for the same source (e.g. toggling
        // a Todo/Task/Project incomplete then complete again), and every ExperienceEntry created during
        // mutation would silently never reach the database at all, since nothing else ever adds it to
        // the DbSet.
        var existingExperienceEntries = await context.ExperienceEntries
            .AsNoTracking()
            .Where(entry => entry.UserId == userId)
            .ToListAsync(cancellationToken);
        user.Experience.Hydrate(existingExperienceEntries);

        mutation(user);

        var existingExperienceEntryIds = existingExperienceEntries.Select(entry => entry.Id).ToHashSet();
        foreach (var entry in user.Experience.Entries)
        {
            if (!existingExperienceEntryIds.Contains(entry.Id))
            {
                context.ExperienceEntries.Add(entry);
            }
        }

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }
}
