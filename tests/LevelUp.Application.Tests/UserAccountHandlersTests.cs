using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Handlers;
using LevelUp.Application.Features.Users.Queries;
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
        var repository = CreateRepository(passwordService.Hash("Current123"), out var context);
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository, passwordService, context);

        await handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Current123", "NewPassword456", "NewPassword456")),
            TestContext.Current.CancellationToken);

        Assert.Equal("hash:NewPassword456", repository.Data.CurrentUser!.PasswordHash);
        Assert.Equal(2, repository.Data.CurrentUser.SessionVersion);
    }

    [Fact]
    public async Task ChangePassword_RejectsIncorrectCurrentPassword()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"), out var context);
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository, passwordService, context);

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
        var repository = CreateRepository(passwordService.Hash("Current123"), out var context);
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository, passwordService, context);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Current123", "Current123", "Current123")),
            TestContext.Current.CancellationToken));

        Assert.Equal("The new password must be different from the current password.", exception.Message);
    }

    [Fact]
    public async Task UpdateProfile_ChangesNameAndEmail()
    {
        var repository = CreateRepository("hash:Current123", out var context);
        var handler = new UpdateCurrentUserAccountCommandHandler(repository, context);

        await handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago Arrigoni", "tiago@levelup.invalid")),
            TestContext.Current.CancellationToken);

        Assert.Equal("Tiago Arrigoni", repository.Data.CurrentUser!.Name);
        Assert.Equal("tiago@levelup.invalid", repository.Data.CurrentUser.Email);
    }

    [Fact]
    public async Task UpdateProfile_RejectsEmailAlreadyUsedByAnotherUser()
    {
        var repository = CreateRepository("hash:Current123", out var context);
        repository.Data.AddUser(User.Create("Other User", "other@levelup.invalid"));
        var handler = new UpdateCurrentUserAccountCommandHandler(repository, context);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago", "other@levelup.invalid")),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdatePreferences_ChangesLanguageAndTheme()
    {
        var repository = CreateRepository("hash:Current123", out var context);
        var handler = new UpdateCurrentUserPreferencesCommandHandler(repository, context);

        await handler.Handle(
            new UpdateCurrentUserPreferencesCommand(new(UserLanguage.Portuguese, UserTheme.Dark)),
            TestContext.Current.CancellationToken);

        Assert.Equal(UserLanguage.Portuguese, repository.Data.CurrentUser!.Language);
        Assert.Equal(UserTheme.Dark, repository.Data.CurrentUser.Theme);
    }

    [Fact]
    public async Task CompleteOnboarding_MarksCurrentUserAsCompleted()
    {
        var repository = CreateRepository("hash:Current123", out var context);
        var handler = new CompleteCurrentUserOnboardingCommandHandler(repository, context);

        await handler.Handle(
            new CompleteCurrentUserOnboardingCommand(),
            TestContext.Current.CancellationToken);

        Assert.True(repository.Data.CurrentUser!.HasCompletedOnboarding);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsIdentityOnlyData()
    {
        var repository = CreateRepository("hash:Current123", out var context);
        var handler = new GetCurrentUserQueryHandler(repository, context);

        var response = await handler.Handle(new GetCurrentUserQuery(), TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(repository.Data.CurrentUser!.Id, response.Id);
        Assert.Equal(repository.Data.CurrentUser.Email, response.Email);
        Assert.Equal(repository.Data.CurrentUser.IsActive, response.IsActive);
        Assert.Equal(repository.Data.CurrentUser.HasCompletedOnboarding, response.HasCompletedOnboarding);
        Assert.Equal(repository.Data.CurrentUser.IsEmailConfirmed, response.IsEmailConfirmed);
    }

    private static FakeLevelUpRepository CreateRepository(string passwordHash, out FakeCurrentUserContext context)
    {
        var repository = new FakeLevelUpRepository();
        var user = User.Create("Test User", "test@levelup.invalid", passwordHash);
        repository.Data.AddUser(user);
        context = new FakeCurrentUserContext(user.Id);
        return repository;
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";

        public bool Verify(string password, string passwordHash) =>
            string.Equals(passwordHash, Hash(password), StringComparison.Ordinal);

        public bool NeedsRehash(string passwordHash) => false;
    }
}
