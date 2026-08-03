using BeeDay.Application.Common.Contracts;
using BeeDay.Infrastructure.Persistence.SqlServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BeeDay.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Coordinates the 8 <c>Ef*Repository</c> adapters against a single <see cref="LevelUpDbContext"/> for
/// the lifetime of this instance — created fresh per unit of work (never resolved as a long-lived
/// scoped/singleton object; see <c>InfrastructureServiceCollectionExtensions</c>'s
/// <c>AddTransient&lt;IUnitOfWork, EfUnitOfWork&gt;</c> registration), disposed by the caller via
/// <c>await using</c> once the unit of work is done. Every repository property below is bound to the
/// same context via each adapter's internal shared-context constructor, so their writes can be
/// coordinated in an explicit transaction (<see cref="BeginTransactionAsync"/>) instead of each
/// committing independently.
/// </summary>
internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly LevelUpDbContext context;
    private IDbContextTransaction? transaction;

    private IUserRepository? users;
    private IUserTokenRepository? userTokens;
    private IHabitRepository? habits;
    private IRecurringTaskRepository? recurringTasks;
    private IProjectRepository? projects;
    private IWalletRepository? wallets;
    private IWalletTagRepository? walletTags;
    private ITransactionRepository? transactions;

    public EfUnitOfWork(IDbContextFactory<LevelUpDbContext> contextFactory)
    {
        // Synchronous CreateDbContext(): constructing the context does not open a connection, so no
        // async/await is needed here — keeps the 8 repository properties below plain synchronous
        // properties instead of async-returning methods.
        context = contextFactory.CreateDbContext();
    }

    public IUserRepository Users => users ??= new EfUserRepository(context);

    public IUserTokenRepository UserTokens => userTokens ??= new EfUserTokenRepository(context);

    public IHabitRepository Habits => habits ??= new EfHabitRepository(context);

    public IRecurringTaskRepository RecurringTasks => recurringTasks ??= new EfRecurringTaskRepository(context);

    public IProjectRepository Projects => projects ??= new EfProjectRepository(context);

    public IWalletRepository Wallets => wallets ??= new EfWalletRepository(context);

    public IWalletTagRepository WalletTags => walletTags ??= new EfWalletTagRepository(context);

    public ITransactionRepository Transactions => transactions ??= new EfTransactionRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is null)
        {
            throw new InvalidOperationException(
                "No active transaction to commit — call BeginTransactionAsync first.");
        }

        await transaction.CommitAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is null)
        {
            throw new InvalidOperationException(
                "No active transaction to roll back — call BeginTransactionAsync first.");
        }

        await transaction.RollbackAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (transaction is not null)
        {
            // Disposing a transaction that was never committed rolls it back automatically (EF Core's
            // own behavior) — the same "DisposeAsync without a prior CommitAsync persists nothing"
            // abort semantics already approved for IHabitProgressionScope
            // (07-persistence-contracts.md §9).
            await transaction.DisposeAsync();
        }

        await context.DisposeAsync();
    }
}
