using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace BeeDay.Infrastructure.Tests.Persistence.SqlServer.Repositories;

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

    [Fact]
    public async Task UpdateAsync_PersistsTheMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserTokenRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var token = UserToken.Create(userId, UserTokenType.EmailConfirmation, "hash-1", now, now.AddHours(1));
        await repository.AddAsync(token, cancellationToken);

        await repository.UpdateAsync(
            token.Id,
            t => t.MarkAsUsed(UserTokenType.EmailConfirmation, now),
            cancellationToken);

        var loaded = await repository.GetByHashAsync("hash-1", UserTokenType.EmailConfirmation, cancellationToken);
        Assert.True(loaded!.IsUsed);
    }

    [Fact]
    public async Task RevokeActiveAsync_RevokesEveryActiveTokenOfThatType()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserTokenRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var first = UserToken.Create(userId, UserTokenType.PasswordReset, "first", now, now.AddHours(1));
        var second = UserToken.Create(userId, UserTokenType.PasswordReset, "second", now, now.AddHours(1));
        var third = UserToken.Create(userId, UserTokenType.PasswordReset, "third", now, now.AddHours(1));
        await repository.AddAsync(first, cancellationToken);
        await repository.AddAsync(second, cancellationToken);
        await repository.AddAsync(third, cancellationToken);

        await repository.RevokeActiveAsync(userId, UserTokenType.PasswordReset, now, cancellationToken);

        var stillActive = await repository.ListActiveAsync(userId, UserTokenType.PasswordReset, cancellationToken);
        Assert.Empty(stillActive);
        Assert.True((await repository.GetByHashAsync("first", UserTokenType.PasswordReset, cancellationToken))!.IsRevoked);
        Assert.True((await repository.GetByHashAsync("second", UserTokenType.PasswordReset, cancellationToken))!.IsRevoked);
        Assert.True((await repository.GetByHashAsync("third", UserTokenType.PasswordReset, cancellationToken))!.IsRevoked);
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
