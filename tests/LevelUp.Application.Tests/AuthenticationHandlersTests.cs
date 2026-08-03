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
        var unitOfWork = new FakeUnitOfWork();
        var user = User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123"));
        user.ConfirmEmail(user.CreatedAtUtc);
        unitOfWork.UsersData.Add(user);
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        var result = await handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("TIAGO@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken);

        Assert.Equal(user.Id, result.Id);
        Assert.NotNull(unitOfWork.UsersData.Single(candidate => candidate.Id == user.Id).LastLoginAtUtc);
    }

    [Fact]
    public async Task Authenticate_RejectsInvalidPassword()
    {
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.UsersData.Add(User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123")));
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "WrongPassword")),
            TestContext.Current.CancellationToken));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task Authenticate_RejectsInactiveUser()
    {
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        var user = User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123"));
        user.SetActive(false);
        unitOfWork.UsersData.Add(user);
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Authenticate_RejectsUnconfirmedEmail()
    {
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        unitOfWork.UsersData.Add(User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123")));
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task Authenticate_ReturnsCurrentSessionVersion()
    {
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        var user = User.Create("Tiago", "tiago@levelup.invalid", passwordService.Hash("Password123"));
        user.ConfirmEmail(user.CreatedAtUtc);
        user.InvalidateSessions();
        unitOfWork.UsersData.Add(user);
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        var result = await handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken);

        Assert.Equal(user.SessionVersion, result.SessionVersion);
    }

    [Fact]
    public async Task Authenticate_RehashesPasswordWhenServiceRequestsIt()
    {
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        var originalHash = passwordService.Hash("Password123");
        var user = User.Create("Tiago", "tiago@levelup.invalid", originalHash);
        user.ConfirmEmail(user.CreatedAtUtc);
        var sessionVersionBeforeLogin = user.SessionVersion;
        unitOfWork.UsersData.Add(user);
        passwordService.RehashNeeded = true;
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        await handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken);

        var stored = unitOfWork.UsersData.Single(candidate => candidate.Id == user.Id);
        Assert.NotEqual(originalHash, stored.PasswordHash);
        Assert.Equal(2, passwordService.HashCallCount);
        Assert.Equal(sessionVersionBeforeLogin, stored.SessionVersion);
    }

    [Fact]
    public async Task Authenticate_DoesNotRehashWhenServiceDoesNotRequestIt()
    {
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        var originalHash = passwordService.Hash("Password123");
        var user = User.Create("Tiago", "tiago@levelup.invalid", originalHash);
        user.ConfirmEmail(user.CreatedAtUtc);
        unitOfWork.UsersData.Add(user);
        var handler = new AuthenticateUserCommandHandler(unitOfWork.Users, passwordService);

        await handler.Handle(
            new AuthenticateUserCommand(new AuthenticateUserRequest("tiago@levelup.invalid", "Password123")),
            TestContext.Current.CancellationToken);

        var stored = unitOfWork.UsersData.Single(candidate => candidate.Id == user.Id);
        Assert.Equal(originalHash, stored.PasswordHash);
        Assert.Equal(1, passwordService.HashCallCount);
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public bool RehashNeeded { get; set; }
        public int HashCallCount { get; private set; }

        public string Hash(string password)
        {
            HashCallCount++;
            return $"hash:{password}:{HashCallCount}";
        }

        public bool Verify(string password, string passwordHash) =>
            passwordHash.StartsWith($"hash:{password}:", StringComparison.Ordinal);

        public bool NeedsRehash(string passwordHash) => RehashNeeded;
    }

}
