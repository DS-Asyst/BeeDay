using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfUserTokenRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetByHashAsync_RoundTripsTheSameToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserTokenRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var token = UserToken.Create(userId, UserTokenType.EmailConfirmation, "hash-1", now, now.AddHours(1));

        await repository.AddAsync(token, cancellationToken);
        var loaded = await repository.GetByHashAsync("hash-1", UserTokenType.EmailConfirmation, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(token.Id, loaded!.Id);
        Assert.Equal(userId, loaded.UserId);
    }

    [Fact]
    public async Task ListActiveAsync_ExcludesUsedRevokedAndExpiredTokens()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserTokenRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var active = UserToken.Create(userId, UserTokenType.PasswordReset, "active", now, now.AddHours(1));

        var used = UserToken.Create(userId, UserTokenType.PasswordReset, "used", now, now.AddHours(1));
        used.MarkAsUsed(UserTokenType.PasswordReset, now);

        var revoked = UserToken.Create(userId, UserTokenType.PasswordReset, "revoked", now, now.AddHours(1));
        revoked.Revoke(now);

        var expired = UserToken.Create(userId, UserTokenType.PasswordReset, "expired", now.AddHours(-2), now.AddHours(-1));

        await repository.AddAsync(active, cancellationToken);
        await repository.AddAsync(used, cancellationToken);
        await repository.AddAsync(revoked, cancellationToken);
        await repository.AddAsync(expired, cancellationToken);

        var activeTokens = await repository.ListActiveAsync(userId, UserTokenType.PasswordReset, cancellationToken);

        Assert.Single(activeTokens);
        Assert.Equal(active.Id, activeTokens[0].Id);
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
