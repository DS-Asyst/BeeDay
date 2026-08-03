using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfTransactionRepository : EfRepositoryBase, ITransactionRepository
{
    public EfTransactionRepository(IDbContextFactory<BeeDayDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfTransactionRepository(BeeDayDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<Transaction?> GetAsync(
        Guid walletId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                transaction => transaction.WalletId == walletId && transaction.Id == transactionId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> ListByTagAsync(
        Guid walletTagId,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.WalletTagId == walletTagId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        lease.Context.Transactions.Add(transaction);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task RemoveAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        var tracked = await lease.Context.Transactions
            .SingleAsync(existing => existing.Id == transaction.Id, cancellationToken);
        lease.Context.Transactions.Remove(tracked);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid walletId,
        Guid transactionId,
        Action<Transaction> mutation,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfUserRepository.UpdateAsync for why tracked-and-mutated-within-this-call, not a
        // disconnected Save.
        var transaction = await context.Transactions
            .SingleAsync(
                existing => existing.WalletId == walletId && existing.Id == transactionId,
                cancellationToken);
        mutation(transaction);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task ClearTagReferencesAsync(Guid walletTagId, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // Mirrors LevelUpData.RemoveWalletTag exactly: every Transaction referencing a deleted
        // WalletTag has its reference cleared, never the Transaction itself.
        var affected = await context.Transactions
            .Where(transaction => transaction.WalletTagId == walletTagId)
            .ToListAsync(cancellationToken);

        foreach (var transaction in affected)
        {
            transaction.RemoveTag();
        }

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }
}
