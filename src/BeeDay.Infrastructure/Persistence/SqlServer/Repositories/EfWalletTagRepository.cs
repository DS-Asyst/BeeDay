using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfWalletTagRepository : EfRepositoryBase, IWalletTagRepository
{
    public EfWalletTagRepository(IDbContextFactory<BeeDayDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfWalletTagRepository(BeeDayDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<WalletTag?> GetAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.WalletTags
            .AsNoTracking()
            .FirstOrDefaultAsync(tag => tag.UserId == userId && tag.Id == tagId, cancellationToken);
    }

    public async Task<IReadOnlyList<WalletTag>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.WalletTags
            .AsNoTracking()
            .Where(tag => tag.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNameInUseAsync(
        Guid userId,
        string normalizedName,
        Guid? excludingTagId = null,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.WalletTags
            .AsNoTracking()
            .Where(tag => tag.UserId == userId && tag.Name == normalizedName)
            .Where(tag => excludingTagId == null || tag.Id != excludingTagId)
            .AnyAsync(cancellationToken);
    }

    public async Task AddAsync(WalletTag tag, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        lease.Context.WalletTags.Add(tag);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task RemoveAsync(WalletTag tag, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        // UserId filter is defense-in-depth (EPIC 30 Sprint 30.22, BD30-F053) — see
        // EfHabitRepository.RemoveAsync for the full rationale.
        var tracked = await lease.Context.WalletTags.SingleAsync(
            existing => existing.Id == tag.Id && existing.UserId == tag.UserId, cancellationToken);
        lease.Context.WalletTags.Remove(tracked);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid userId,
        Guid tagId,
        Action<WalletTag> mutation,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfUserRepository.UpdateAsync for why tracked-and-mutated-within-this-call, not a
        // disconnected Save.
        var tag = await context.WalletTags
            .SingleAsync(existing => existing.UserId == userId && existing.Id == tagId, cancellationToken);
        mutation(tag);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }
}
