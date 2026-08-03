using Microsoft.EntityFrameworkCore;

namespace BeeDay.Infrastructure.Persistence.SqlServer.Repositories;

/// <summary>
/// Shared context-acquisition plumbing for the 8 <c>Ef*Repository</c> adapters — not a generic
/// repository abstraction (it knows nothing about entity shape or CRUD operations), only "where does
/// this call's <see cref="LevelUpDbContext"/> come from, and who is responsible for disposing it."
/// A repository built with the factory constructor creates and owns a fresh context per call (Sprint
/// 14.4 behavior, unchanged for standalone/DI use); one built with the shared-context constructor
/// (used exclusively by <see cref="EfUnitOfWork"/>) reuses that single context across every call and
/// never disposes it — <see cref="EfUnitOfWork"/> owns that lifetime instead.
/// </summary>
internal abstract class EfRepositoryBase
{
    private readonly IDbContextFactory<LevelUpDbContext>? contextFactory;
    private readonly LevelUpDbContext? sharedContext;

    protected EfRepositoryBase(IDbContextFactory<LevelUpDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    protected EfRepositoryBase(LevelUpDbContext sharedContext)
    {
        this.sharedContext = sharedContext;
    }

    protected async Task<DbContextLease> AcquireContextAsync(CancellationToken cancellationToken)
    {
        if (sharedContext is not null)
        {
            return new DbContextLease(sharedContext, ownsContext: false);
        }

        var context = await contextFactory!.CreateDbContextAsync(cancellationToken);
        return new DbContextLease(context, ownsContext: true);
    }
}

/// <summary>
/// A <see cref="LevelUpDbContext"/> handed to a repository method, together with whether that method is
/// responsible for disposing it (own, per-call context) or not (shared context owned by
/// <see cref="EfUnitOfWork"/>).
/// </summary>
internal readonly struct DbContextLease(LevelUpDbContext context, bool ownsContext) : IAsyncDisposable
{
    public LevelUpDbContext Context { get; } = context;

    public async ValueTask DisposeAsync()
    {
        if (ownsContext)
        {
            await Context.DisposeAsync();
        }
    }
}
