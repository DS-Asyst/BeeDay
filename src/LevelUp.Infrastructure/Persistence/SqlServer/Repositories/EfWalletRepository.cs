using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfWalletRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : IWalletRepository
{
    public async Task<Wallet?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.Wallets.Add(wallet);
        await context.SaveChangesAsync(cancellationToken);
    }
}
