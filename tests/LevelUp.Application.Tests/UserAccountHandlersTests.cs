using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Handlers;
using LevelUp.Application.Features.Users.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class UserAccountHandlersTests
{
    [Fact]
    public async Task ChangePassword_ReplacesHashWhenCurrentPasswordIsCorrect()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"));
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository, passwordService);

        await handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Current123", "NewPassword456", "NewPassword456")),
            TestContext.Current.CancellationToken);

        Assert.Equal("hash:NewPassword456", repository.Data.CurrentUser!.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_RejectsIncorrectCurrentPassword()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"));
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository, passwordService);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Wrong123", "NewPassword456", "NewPassword456")),
            TestContext.Current.CancellationToken));

        Assert.Equal("The current password is incorrect.", exception.Message);
        Assert.Equal("hash:Current123", repository.Data.CurrentUser!.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_RejectsCurrentPasswordReuse()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"));
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository, passwordService);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Current123", "Current123", "Current123")),
            TestContext.Current.CancellationToken));

        Assert.Equal("The new password must be different from the current password.", exception.Message);
    }

    [Fact]
    public async Task UpdateProfile_ChangesNameAndEmail()
    {
        var repository = CreateRepository("hash:Current123");
        var handler = new UpdateCurrentUserAccountCommandHandler(repository);

        await handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago Arrigoni", "tiago@levelup.invalid")),
            TestContext.Current.CancellationToken);

        Assert.Equal("Tiago Arrigoni", repository.Data.CurrentUser!.Name);
        Assert.Equal("tiago@levelup.invalid", repository.Data.CurrentUser.Email);
    }

    [Fact]
    public async Task UpdateProfile_RejectsEmailAlreadyUsedByAnotherUser()
    {
        var repository = CreateRepository("hash:Current123");
        repository.Data.AddUser(User.Create("Other User", "other@levelup.invalid"));
        var handler = new UpdateCurrentUserAccountCommandHandler(repository);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago", "other@levelup.invalid")),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdatePreferences_ChangesLanguageAndTheme()
    {
        var repository = CreateRepository("hash:Current123");
        var handler = new UpdateCurrentUserPreferencesCommandHandler(repository);

        await handler.Handle(
            new UpdateCurrentUserPreferencesCommand(new(UserLanguage.Portuguese, UserTheme.Dark)),
            TestContext.Current.CancellationToken);

        Assert.Equal(UserLanguage.Portuguese, repository.Data.CurrentUser!.Language);
        Assert.Equal(UserTheme.Dark, repository.Data.CurrentUser.Theme);
    }

    [Fact]
    public async Task CompleteOnboarding_MarksCurrentUserAsCompleted()
    {
        var repository = CreateRepository("hash:Current123");
        var handler = new CompleteCurrentUserOnboardingCommandHandler(repository);

        await handler.Handle(
            new CompleteCurrentUserOnboardingCommand(),
            TestContext.Current.CancellationToken);

        Assert.True(repository.Data.CurrentUser!.HasCompletedOnboarding);
    }

    private static TestRepository CreateRepository(string passwordHash)
    {
        var repository = new TestRepository();
        var user = User.Create("Test User", "test@levelup.invalid", passwordHash);
        repository.Data.AddUser(user);
        repository.Data.SetCurrentUser(user.Id);
        return repository;
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

        public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Data);

        public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
        {
            mutation(Data);
            return Task.CompletedTask;
        }
    }
}
