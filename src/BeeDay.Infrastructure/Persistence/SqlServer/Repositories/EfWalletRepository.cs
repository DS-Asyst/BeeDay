using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfWalletRepository : EfRepositoryBase, IWalletRepository
{
    public EfWalletRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfWalletRepository(LevelUpDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<Wallet?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(wallet => wallet.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        lease.Context.Wallets.Add(wallet);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task UpdateAsync(Guid userId, Action<Wallet> mutation, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfUserRepository.UpdateAsync for why tracked-and-mutated-within-this-call, not a
        // disconnected Save.
        var wallet = await context.Wallets.SingleAsync(existing => existing.UserId == userId, cancellationToken);
        mutation(wallet);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }
}
