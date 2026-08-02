using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

[Collection("EfLocalDb")]
public sealed class EfWalletTagRepositoryTests : EfLocalDbTestBase
{
    [Fact]
    public async Task AddAsync_ThenGetAsyncAndListAsync_RoundTripTheSameTag()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletTagRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var tag = WalletTag.Create(userId, "Groceries");

        await repository.AddAsync(tag, cancellationToken);
        var loaded = await repository.GetAsync(userId, tag.Id, cancellationToken);
        var listed = await repository.ListAsync(userId, cancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal("Groceries", loaded!.Name);
        Assert.Single(listed);
    }

    [Fact]
    public async Task IsNameInUseAsync_RespectsExcludingTagId()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletTagRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var tag = WalletTag.Create(userId, "Groceries");
        await repository.AddAsync(tag, cancellationToken);

        Assert.True(await repository.IsNameInUseAsync(userId, "Groceries", cancellationToken: cancellationToken));
        Assert.False(await repository.IsNameInUseAsync(userId, "Groceries", tag.Id, cancellationToken));
        Assert.False(await repository.IsNameInUseAsync(userId, "Rent", cancellationToken: cancellationToken));
    }

    [Fact]
    public async Task RemoveAsync_DeletesTheTag()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var repository = new EfWalletTagRepository(ContextFactory);
        var userId = await CreateUserAsync(cancellationToken);
        var tag = WalletTag.Create(userId, "Groceries");
        await repository.AddAsync(tag, cancellationToken);

        await repository.RemoveAsync(tag, cancellationToken);
        var loaded = await repository.GetAsync(userId, tag.Id, cancellationToken);

        Assert.Null(loaded);
    }

    private async Task<Guid> CreateUserAsync(CancellationToken cancellationToken)
    {
        var user = User.Create($"Test User {Guid.NewGuid():N}", $"{Guid.NewGuid():N}@example.com");
        await new EfUserRepository(ContextFactory).AddAsync(user, cancellationToken);
        return user.Id;
    }
}
