using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfWalletTagRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : IWalletTagRepository
{
    public async Task<WalletTag?> GetAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.WalletTags
            .AsNoTracking()
            .FirstOrDefaultAsync(tag => tag.UserId == userId && tag.Id == tagId, cancellationToken);
    }

    public async Task<IReadOnlyList<WalletTag>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.WalletTags
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
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.WalletTags
            .AsNoTracking()
            .Where(tag => tag.UserId == userId && tag.Name == normalizedName)
            .Where(tag => excludingTagId == null || tag.Id != excludingTagId)
            .AnyAsync(cancellationToken);
    }

    public async Task AddAsync(WalletTag tag, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.WalletTags.Add(tag);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(WalletTag tag, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tracked = await context.WalletTags.SingleAsync(existing => existing.Id == tag.Id, cancellationToken);
        context.WalletTags.Remove(tracked);
        await context.SaveChangesAsync(cancellationToken);
    }
}
