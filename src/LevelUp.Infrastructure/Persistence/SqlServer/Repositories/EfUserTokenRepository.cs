using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfUserTokenRepository : EfRepositoryBase, IUserTokenRepository
{
    public EfUserTokenRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : base(contextFactory)
    {
    }

    internal EfUserTokenRepository(LevelUpDbContext sharedContext) : base(sharedContext)
    {
    }

    public async Task<UserToken?> GetByHashAsync(
        string tokenHash,
        UserTokenType type,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        return await lease.Context.UserTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash && token.Type == type, cancellationToken);
    }

    public async Task<IReadOnlyList<UserToken>> ListActiveAsync(
        Guid userId,
        UserTokenType type,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var nowUtc = DateTimeOffset.UtcNow;

        return await lease.Context.UserTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId
                && token.Type == type
                && token.UsedAtUtc == null
                && token.RevokedAtUtc == null
                && token.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);

        lease.Context.UserTokens.Add(token);
        await EfConcurrencySaveChanges.ExecuteAsync(lease.Context, cancellationToken);
    }

    public async Task UpdateAsync(
        Guid tokenId,
        Action<UserToken> mutation,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        // See EfUserRepository.UpdateAsync for why tracked-and-mutated-within-this-call, not a
        // disconnected Save.
        var token = await context.UserTokens.SingleAsync(existing => existing.Id == tokenId, cancellationToken);
        mutation(token);
        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }

    public async Task RevokeActiveAsync(
        Guid userId,
        UserTokenType type,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await AcquireContextAsync(cancellationToken);
        var context = lease.Context;

        var activeTokens = await context.UserTokens
            .Where(token => token.UserId == userId
                && token.Type == type
                && token.UsedAtUtc == null
                && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedAtUtc);
        }

        await EfConcurrencySaveChanges.ExecuteAsync(context, cancellationToken);
    }
}
