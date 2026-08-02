using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Repositories;

internal sealed class EfUserTokenRepository(IDbContextFactory<LevelUpDbContext> contextFactory) : IUserTokenRepository
{
    public async Task<UserToken?> GetByHashAsync(
        string tokenHash,
        UserTokenType type,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.UserTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash && token.Type == type, cancellationToken);
    }

    public async Task<IReadOnlyList<UserToken>> ListActiveAsync(
        Guid userId,
        UserTokenType type,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var nowUtc = DateTimeOffset.UtcNow;

        return await context.UserTokens
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
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        context.UserTokens.Add(token);
        await context.SaveChangesAsync(cancellationToken);
    }
}
