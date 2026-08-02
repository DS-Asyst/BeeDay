using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfTransactionRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : ITransactionRepository
{
    public async Task<Transaction?> GetAsync(
        Guid walletId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                transaction => transaction.WalletId == walletId && transaction.Id == transactionId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> ListByTagAsync(
        Guid walletTagId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.WalletTagId == walletTagId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var tracked = await context.Transactions
            .SingleAsync(existing => existing.Id == transaction.Id, cancellationToken);
        context.Transactions.Remove(tracked);
        await context.SaveChangesAsync(cancellationToken);
    }
}
