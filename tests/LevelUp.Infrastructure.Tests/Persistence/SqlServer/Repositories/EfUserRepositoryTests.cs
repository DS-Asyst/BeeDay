using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfUserRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundTripsTheSameUser()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserRepository(ContextFactory);
        var user = User.Create("Ada Lovelace", "ada@example.com");

        await repository.AddAsync(user, cancellationToken);
        var loaded = await repository.GetByIdAsync(user.Id, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(user.Id, loaded!.Id);
        Assert.Equal("ada@example.com", loaded.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_FindsTheMatchingUser()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserRepository(ContextFactory);
        var user = User.Create("Grace Hopper", "grace@example.com");
        await repository.AddAsync(user, cancellationToken);

        var loaded = await repository.GetByEmailAsync("grace@example.com", cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(user.Id, loaded!.Id);
    }

    [Fact]
    public async Task IsEmailInUseAsync_RespectsExcludingUserId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserRepository(ContextFactory);
        var user = User.Create("Katherine Johnson", "katherine@example.com");
        await repository.AddAsync(user, cancellationToken);

        Assert.True(await repository.IsEmailInUseAsync("katherine@example.com", cancellationToken: cancellationToken));
        Assert.False(await repository.IsEmailInUseAsync("katherine@example.com", user.Id, cancellationToken));
        Assert.False(await repository.IsEmailInUseAsync("nobody@example.com", cancellationToken: cancellationToken));
    }

    [Fact]
    public async Task IsNicknameInUseAsync_RespectsExcludingUserId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfUserRepository(ContextFactory);
        var user = User.Create("Margaret Hamilton", "margaret@example.com");
        await repository.AddAsync(user, cancellationToken);

        // Nickname completion is an internal Domain flow (User.CompleteProfile) not reachable from this
        // test assembly — the property entry is set directly through EF Core's own tracking API instead,
        // which works regardless of the private setter, purely to set up repository-level test data.
        await using (var context = await ContextFactory.CreateDbContextAsync(cancellationToken))
        {
            var tracked = await context.Users.SingleAsync(existing => existing.Id == user.Id, cancellationToken);
            context.Entry(tracked).Property(existing => existing.Nickname).CurrentValue = "margaret";
            await context.SaveChangesAsync(cancellationToken);
        }

        Assert.True(await repository.IsNicknameInUseAsync("margaret", cancellationToken: cancellationToken));
        Assert.False(await repository.IsNicknameInUseAsync("margaret", user.Id, cancellationToken));
        Assert.False(await repository.IsNicknameInUseAsync("nobody", cancellationToken: cancellationToken));
    }
}
