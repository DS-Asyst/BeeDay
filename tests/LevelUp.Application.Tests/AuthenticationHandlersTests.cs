using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Authentication.Commands;
using LevelUp.Application.Features.Authentication.Handlers;
using LevelUp.Application.Features.Authentication.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class AuthenticationHandlersTests
{
    [Fact]
    public async Task Authenticate_SelectsUserAndRegistersLogin()
    {
        var passwordService = new FakePasswordService();
        var repository = new TestRepository();
        var user = User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123"));
        user.ConfirmEmail(user.CreatedAtUtc);
        repository.Data.AddUser(user);
        var handler = new AuthenticateUserCommandHandler(repository, passwordService);

        var result = await handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("TIAGO@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Id, repository.Data.CurrentUserId);
        Assert.NotNull(repository.Data.CurrentUser!.LastLoginAtUtc);
    }

    [Fact]
    public async Task Authenticate_RejectsInvalidPassword()
    {
        var passwordService = new FakePasswordService();
        var repository = new TestRepository();
        repository.Data.AddUser(User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123")));
        var handler = new AuthenticateUserCommandHandler(repository, passwordService);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "WrongPassword")),
            TestContext.Current.CancellationToken));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task Authenticate_RejectsInactiveUser()
    {
        var passwordService = new FakePasswordService();
        var repository = new TestRepository();
        var user = User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123"));
        user.SetActive(false);
        repository.Data.AddUser(user);
        var handler = new AuthenticateUserCommandHandler(repository, passwordService);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken));
    }


    [Fact]
    public async Task Authenticate_RejectsUnconfirmedEmail()
    {
        var passwordService = new FakePasswordService();
        var repository = new TestRepository();
        repository.Data.AddUser(User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123")));
        var handler = new AuthenticateUserCommandHandler(repository, passwordService);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken));

        Assert.Equal("Please confirm your email before signing in.", exception.Message);
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";
        public bool Verify(string password, string passwordHash) =>
            string.Equals(passwordHash, Hash(password), StringComparison.Ordinal);
    }

    private sealed class TestRepository : ILevelUpRepository
    {
        public LevelUpData Data { get; } = new();
        public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Data);
        public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
        {
            mutation(Data);
            return Task.CompletedTask;
        }
    }
}
