using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Handlers;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Application.Tests;

public sealed class UserAccountHandlersTests
{
    [Fact]
    public async Task ChangePassword_ReplacesHashWhenCurrentPasswordIsCorrect()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"), out var context, out var user);
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository.Users, passwordService, context);

        await handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Current123", "NewPassword456", "NewPassword456")),
            TestContext.Current.CancellationToken);

        Assert.Equal("hash:NewPassword456", user.PasswordHash);
        Assert.Equal(2, user.SessionVersion);
    }

    [Fact]
    public async Task ChangePassword_RejectsIncorrectCurrentPassword()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"), out var context, out var user);
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository.Users, passwordService, context);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Wrong123", "NewPassword456", "NewPassword456")),
            TestContext.Current.CancellationToken));

        Assert.Equal("The current password is incorrect.", exception.Message);
        Assert.Equal("hash:Current123", user.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_RejectsCurrentPasswordReuse()
    {
        var passwordService = new FakePasswordService();
        var repository = CreateRepository(passwordService.Hash("Current123"), out var context, out _);
        var handler = new ChangeCurrentUserPasswordCommandHandler(repository.Users, passwordService, context);

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ChangeCurrentUserPasswordCommand(new("Current123", "Current123", "Current123")),
            TestContext.Current.CancellationToken));

        Assert.Equal("The new password must be different from the current password.", exception.Message);
    }

    [Fact]
    public async Task UpdateProfile_ChangesNameAndEmail()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = new UpdateCurrentUserAccountCommandHandler(repository.Users, context);

        await handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago Arrigoni", "tiago@beeday.invalid")),
            TestContext.Current.CancellationToken);

        Assert.Equal("Tiago Arrigoni", user.Name);
        Assert.Equal("tiago@beeday.invalid", user.Email);
    }

    [Fact]
    public async Task UpdateProfile_RejectsEmailAlreadyUsedByAnotherUser()
    {
        var repository = CreateRepository("hash:Current123", out var context, out _);
        repository.UsersData.Add(User.Create("Other User", "other@beeday.invalid"));
        var handler = new UpdateCurrentUserAccountCommandHandler(repository.Users, context);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago", "other@beeday.invalid")),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdatePreferences_ChangesLanguageAndTheme()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = new UpdateCurrentUserPreferencesCommandHandler(repository.Users, context);

        await handler.Handle(
            new UpdateCurrentUserPreferencesCommand(new(UserLanguage.Portuguese, UserTheme.Dark)),
            TestContext.Current.CancellationToken);

        Assert.Equal(UserLanguage.Portuguese, user.Language);
        Assert.Equal(UserTheme.Dark, user.Theme);
    }

    [Fact]
    public async Task UpdateAvatar_ChangesTheCurrentUsersAvatar()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = new UpdateCurrentUserAvatarCommandHandler(repository.Users, context);

        await handler.Handle(
            new UpdateCurrentUserAvatarCommand(new UpdateUserAvatarRequest("avatar-42.png")),
            TestContext.Current.CancellationToken);

        Assert.Equal("avatar-42.png", user.Avatar);
    }

    [Fact]
    public async Task CompleteOnboarding_MarksCurrentUserAsCompleted()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = new CompleteCurrentUserOnboardingCommandHandler(repository.Users, context);

        await handler.Handle(
            new CompleteCurrentUserOnboardingCommand(),
            TestContext.Current.CancellationToken);

        Assert.True(user.HasCompletedOnboarding);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsIdentityOnlyData()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = new GetCurrentUserQueryHandler(repository.Users, context);

        var response = await handler.Handle(new GetCurrentUserQuery(), TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.IsActive, response.IsActive);
        Assert.Equal(user.HasCompletedOnboarding, response.HasCompletedOnboarding);
        Assert.Equal(user.IsEmailConfirmed, response.IsEmailConfirmed);
    }

    private static FakeUnitOfWork CreateRepository(string passwordHash, out FakeCurrentUserContext context, out User user)
    {
        var repository = new FakeUnitOfWork();
        user = User.Create("Test User", "test@beeday.invalid", passwordHash);
        repository.UsersData.Add(user);
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
