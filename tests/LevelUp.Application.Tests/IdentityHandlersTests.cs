using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Identity;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Identity.Commands;
using LevelUp.Application.Features.Identity.Handlers;
using LevelUp.Application.Features.Identity.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class IdentityHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ConfirmEmail_ConfirmsUserAndConsumesToken()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(confirmed: false);
        var token = fixture.AddToken(user, UserTokenType.EmailConfirmation, "confirm-token", Now.AddHours(1));
        var handler = new ConfirmEmailCommandHandler(fixture.Repository, fixture.Tokens, fixture.Clock);

        await handler.Handle(
            new ConfirmEmailCommand(new ConfirmEmailRequest("confirm-token")),
            TestContext.Current.CancellationToken);

        Assert.True(user.IsEmailConfirmed);
        Assert.Equal(Now, user.EmailConfirmedAtUtc);
        Assert.True(token.IsUsed);
    }

    [Fact]
    public async Task ConfirmEmail_RejectsExpiredToken()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(confirmed: false);
        fixture.AddToken(user, UserTokenType.EmailConfirmation, "expired-token", Now.AddMinutes(-1));
        var handler = new ConfirmEmailCommandHandler(fixture.Repository, fixture.Tokens, fixture.Clock);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ConfirmEmailCommand(new ConfirmEmailRequest("expired-token")),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResendConfirmation_RevokesPreviousTokenAndSendsNewMessage()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(confirmed: false);
        var previous = fixture.AddToken(user, UserTokenType.EmailConfirmation, "old-token", Now.AddHours(1));
        var handler = new ResendEmailConfirmationCommandHandler(
            fixture.Repository, fixture.Tokens, fixture.Composer, fixture.Email, fixture.Throttle, fixture.Clock);

        await handler.Handle(
            new ResendEmailConfirmationCommand(new ResendEmailConfirmationRequest(user.Email)),
            TestContext.Current.CancellationToken);

        Assert.True(previous.IsRevoked);
        Assert.Equal(2, fixture.Repository.Data.UserTokens.Count);
        Assert.Single(fixture.Email.Messages);
    }

    [Fact]
    public async Task RequestPasswordReset_DoesNotRevealMissingEmail()
    {
        var fixture = new Fixture();
        var handler = new RequestPasswordResetCommandHandler(
            fixture.Repository, fixture.Tokens, fixture.Composer, fixture.Email, fixture.Throttle, fixture.Clock);

        await handler.Handle(
            new RequestPasswordResetCommand(new RequestPasswordResetRequest("missing@levelup.invalid")),
            TestContext.Current.CancellationToken);

        Assert.Empty(fixture.Repository.Data.UserTokens);
        Assert.Empty(fixture.Email.Messages);
    }

    [Fact]
    public async Task RequestPasswordReset_CreatesSingleUseTokenForConfirmedUser()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(confirmed: true);
        var handler = new RequestPasswordResetCommandHandler(
            fixture.Repository, fixture.Tokens, fixture.Composer, fixture.Email, fixture.Throttle, fixture.Clock);

        await handler.Handle(
            new RequestPasswordResetCommand(new RequestPasswordResetRequest(user.Email)),
            TestContext.Current.CancellationToken);

        var token = Assert.Single(fixture.Repository.Data.UserTokens);
        Assert.Equal(UserTokenType.PasswordReset, token.Type);
        Assert.Equal(Now.AddHours(1), token.ExpiresAtUtc);
        Assert.Single(fixture.Email.Messages);
    }

    [Fact]
    public async Task ResetPassword_ChangesPasswordAndConsumesToken()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(confirmed: true);
        var token = fixture.AddToken(user, UserTokenType.PasswordReset, "reset-token", Now.AddHours(1));
        var handler = new ResetPasswordCommandHandler(
            fixture.Repository, fixture.Tokens, fixture.Passwords, fixture.Clock);

        await handler.Handle(
            new ResetPasswordCommand(new ResetPasswordRequest("reset-token", "NewPassword123", "NewPassword123")),
            TestContext.Current.CancellationToken);

        Assert.Equal("hash:NewPassword123", user.PasswordHash);
        Assert.True(token.IsUsed);
        Assert.Equal(2, user.SessionVersion);
    }

    [Fact]
    public async Task ResetPassword_RejectsReusedToken()
    {
        var fixture = new Fixture();
        var user = fixture.AddUser(confirmed: true);
        var token = fixture.AddToken(user, UserTokenType.PasswordReset, "reset-token", Now.AddHours(1));
        token.MarkAsUsed(UserTokenType.PasswordReset, Now.AddMinutes(-1));
        var handler = new ResetPasswordCommandHandler(
            fixture.Repository, fixture.Tokens, fixture.Passwords, fixture.Clock);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new ResetPasswordCommand(new ResetPasswordRequest("reset-token", "NewPassword123", "NewPassword123")),
            TestContext.Current.CancellationToken));
    }

    private sealed class Fixture
    {
        public TestRepository Repository { get; } = new();
        public FakeTokenService Tokens { get; } = new();
        public FakeClock Clock { get; } = new(Now);
        public FakeEmailComposer Composer { get; } = new();
        public FakeEmailSender Email { get; } = new();
        public FakeIdentityRequestThrottle Throttle { get; } = new();
        public FakePasswordService Passwords { get; } = new();

        public User AddUser(bool confirmed)
        {
            var user = User.Create("Tiago", "tiago@levelup.invalid", Passwords.Hash("Password123"), Now.AddHours(-2));
            if (confirmed)
            {
                user.ConfirmEmail(user.CreatedAtUtc);
            }
            Repository.Data.AddUser(user);
            return user;
        }

        public UserToken AddToken(User user, UserTokenType type, string rawToken, DateTimeOffset expiresAt)
        {
            var token = UserToken.Create(user.Id, type, Tokens.HashToken(rawToken), Now.AddHours(-1), expiresAt);
            Repository.Data.AddUserToken(token);
            return token;
        }
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private sealed class FakeTokenService : IUserTokenService
    {
        private int sequence;
        public string GenerateToken() => $"generated-{++sequence}";
        public string HashToken(string token) => $"hash:{token}";
    }


    private sealed class FakeIdentityRequestThrottle : IIdentityRequestThrottle
    {
        public bool TryAcquire(string operation, string subject, TimeSpan cooldown, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            return true;
        }
    }

    private sealed class FakeEmailComposer : IIdentityEmailComposer
    {
        public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken) =>
            new(recipient, "Confirm email", rawToken);
        public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken) =>
            new(recipient, "Reset password", rawToken);
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

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
        public bool NeedsRehash(string passwordHash) => false;
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
