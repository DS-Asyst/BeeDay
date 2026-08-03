namespace BeeDay.Application.Common.Contracts;

/// <summary>
/// Coordinates multiple Aggregate repositories against a single, shared persistence session so their
/// writes can be committed or rolled back together. See docs/architecture/07-persistence-contracts.md
/// §6/§10/§13 — the 8 per-Aggregate ports were deliberately left without a way to persist a mutation on
/// an already-loaded Aggregate, pending this piece. Every repository exposed here operates against the
/// same underlying persistence session for the lifetime of this instance — never a fresh one per call,
/// unlike each port's standalone adapter.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    public IUserRepository Users { get; }

    public IUserTokenRepository UserTokens { get; }

    public IHabitRepository Habits { get; }

    public IRecurringTaskRepository RecurringTasks { get; }

    public IProjectRepository Projects { get; }

    public IWalletRepository Wallets { get; }

    public IWalletTagRepository WalletTags { get; }

    public ITransactionRepository Transactions { get; }

    /// <summary>
    /// Persists whatever is currently tracked in this unit of work's session. Most repository write
    /// methods already call this internally, so this is only needed to persist a mutation made directly
    /// on an entity still tracked from an earlier call within the same unit of work (e.g. right after
    /// <c>Habits.AddAsync(habit)</c>, mutating <c>habit</c> and calling this again) — reads remain
    /// untracked, so this cannot persist a change made to a value obtained from a <c>GetAsync</c>/
    /// <c>ListAsync</c> call.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens an explicit transaction — only needed when two or more <see cref="SaveChangesAsync"/>
    /// calls (whether triggered directly or by a repository write method) must be atomic together. A
    /// single write is already atomic on its own without calling this.
    /// </summary>
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
