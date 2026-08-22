using BeeDay.Application.Common.Identity;
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
    public async Task UpdateAccount_NameOnlyChange_DoesNotRequirePasswordAndKeepsEmailConfirmed()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        user.ConfirmEmail(user.CreatedAtUtc);
        var emailSender = new FakeEmailSender();
        var handler = CreateHandler(repository, context, emailSender);

        await handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago Arrigoni", user.Email)),
            TestContext.Current.CancellationToken);

        Assert.Equal("Tiago Arrigoni", user.Name);
        Assert.True(user.IsEmailConfirmed);
        Assert.Empty(emailSender.Messages);
        Assert.Empty(repository.UserTokensData);
    }

    // BD30-F044 (EPIC 30 Sprint 30.11): before this fix, UpdateCurrentUserAccountCommandHandler let a
    // hijacked session silently repoint Email — with no password check and no confirmation reset —
    // which RequestPasswordResetCommandHandler's IsEmailConfirmed gate then treated as already-verified,
    // a full account-takeover primitive. These four tests prove the closed gap.
    [Fact]
    public async Task UpdateAccount_EmailChange_RejectsAnIncorrectCurrentPassword()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = CreateHandler(repository, context, new FakeEmailSender());

        var exception = await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago", "tiago@beeday.invalid", "Wrong123")),
            TestContext.Current.CancellationToken));

        Assert.Equal("The current password is incorrect.", exception.Message);
        Assert.Equal("test@beeday.invalid", user.Email);
    }

    [Fact]
    public async Task UpdateAccount_EmailChange_RejectsAMissingCurrentPassword()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        var handler = CreateHandler(repository, context, new FakeEmailSender());

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago", "tiago@beeday.invalid")),
            TestContext.Current.CancellationToken));

        Assert.Equal("test@beeday.invalid", user.Email);
    }

    [Fact]
    public async Task UpdateAccount_EmailChange_ResetsConfirmationAndSendsAFreshConfirmationEmailToTheNewAddress()
    {
        var repository = CreateRepository("hash:Current123", out var context, out var user);
        user.ConfirmEmail(user.CreatedAtUtc);
        var emailSender = new FakeEmailSender();
        var handler = CreateHandler(repository, context, emailSender);

        await handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago Arrigoni", "tiago@beeday.invalid", "Current123")),
            TestContext.Current.CancellationToken);

        Assert.Equal("Tiago Arrigoni", user.Name);
        Assert.Equal("tiago@beeday.invalid", user.Email);
        Assert.False(user.IsEmailConfirmed);
        Assert.Null(user.EmailConfirmedAtUtc);
        var message = Assert.Single(emailSender.Messages);
        Assert.Equal("tiago@beeday.invalid", message.Recipient);
        var token = Assert.Single(repository.UserTokensData);
        Assert.Equal(UserTokenType.EmailConfirmation, token.Type);
    }

    [Fact]
    public async Task UpdateProfile_RejectsEmailAlreadyUsedByAnotherUser()
    {
        var repository = CreateRepository("hash:Current123", out var context, out _);
        repository.UsersData.Add(User.Create("Other User", "other@beeday.invalid"));
        var handler = CreateHandler(repository, context, new FakeEmailSender());

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateCurrentUserAccountCommand(new UpdateUserAccountRequest("Tiago", "other@beeday.invalid", "Current123")),
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

    private static UpdateCurrentUserAccountCommandHandler CreateHandler(
        FakeUnitOfWork repository, FakeCurrentUserContext context, IEmailSender emailSender) =>
        new(repository, new FakePasswordService(), new FakeConfirmationIssuer(), emailSender, new FakeClock(DateTimeOffset.UtcNow), context);

    private sealed class FakeConfirmationIssuer : IEmailConfirmationIssuer
    {
        public (UserToken Token, EmailMessage Message) Issue(User user)
        {
            var now = user.CreatedAtUtc;
            var token = UserToken.Create(user.Id, UserTokenType.EmailConfirmation, "hash:confirmation", now, now.AddHours(24));
            return (token, new EmailMessage(user.Email, "Confirm", "Body"));
        }
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public List<EmailMessage> Messages { get; } = [];
        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";

        public bool Verify(string password, string passwordHash) =>
            string.Equals(passwordHash, Hash(password), StringComparison.Ordinal);

        public bool NeedsRehash(string passwordHash) => false;
    }
}
