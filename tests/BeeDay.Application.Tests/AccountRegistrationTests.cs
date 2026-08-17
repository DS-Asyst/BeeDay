using BeeDay.Application.Common.Identity;
using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Handlers;
using BeeDay.Application.Features.Users.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;

namespace BeeDay.Application.Tests;

public sealed class AccountRegistrationTests
{
    [Fact]
    public async Task CreateAccount_CreatesUserWithProfileAtomically()
    {
        var repository = new FakeUnitOfWork();
        var emailSender = new FakeEmailSender();
        var handler = new CreateAccountCommandHandler(
            repository,
            new FakePasswordService(),
            new FakeConfirmationIssuer(),
            emailSender);

        var userId = await handler.Handle(
            new CreateAccountCommand(new CreateAccountRequest(
                "Tiago",
                "tiago@example.com",
                "Password123",
                "tiago")),
            TestContext.Current.CancellationToken);

        var user = Assert.Single(repository.UsersData);
        Assert.Equal(userId, user.Id);
        Assert.True(user.HasProfile);
        Assert.Equal("tiago", user.Nickname);
        Assert.Equal("hash:Password123", user.PasswordHash);
        Assert.Single(repository.UserTokensData);
        Assert.Single(emailSender.Messages);
    }

    // The transaction (unitOfWork.CommitTransactionAsync) commits before emailSender.SendAsync runs,
    // outside the try/finally that owns it — a deliberate, documented boundary (Epic 26, Sprint 26.5;
    // see docs/infrastructure/06-transactional-email.md §12), not an accidental gap: the alternative
    // (rolling back a successful account creation because the *notification* about it failed) would
    // be worse, and the repository already exposes a working resend-confirmation path for this exact
    // case. This test proves the account is not silently lost when delivery fails.
    [Fact]
    public async Task CreateAccount_WhenEmailSendFails_UserAndTokenArePersistedDespiteTheFailure()
    {
        var repository = new FakeUnitOfWork();
        var emailSender = new ThrowingEmailSender();
        var handler = new CreateAccountCommandHandler(
            repository,
            new FakePasswordService(),
            new FakeConfirmationIssuer(),
            emailSender);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateAccountCommand(new CreateAccountRequest(
                "Tiago",
                "tiago@example.com",
                "Password123",
                "tiago")),
            TestContext.Current.CancellationToken));

        var user = Assert.Single(repository.UsersData);
        Assert.True(user.HasProfile);
        Assert.Single(repository.UserTokensData);
    }

    [Fact]
    public async Task CreateUser_CreatesUserAndSendsConfirmation()
    {
        var repository = new FakeUnitOfWork();
        var emailSender = new FakeEmailSender();
        var handler = new CreateUserCommandHandler(
            repository,
            new FakePasswordService(),
            new FakeConfirmationIssuer(),
            emailSender);

        var userId = await handler.Handle(
            new CreateUserCommand(new CreateUserRequest("Tiago", "tiago@example.com", "Password123")),
            TestContext.Current.CancellationToken);

        var user = Assert.Single(repository.UsersData);
        Assert.Equal(userId, user.Id);
        Assert.Single(repository.UserTokensData);
        Assert.Single(emailSender.Messages);
    }

    [Fact]
    public async Task CreateUser_WhenEmailSendFails_UserAndTokenArePersistedDespiteTheFailure()
    {
        var repository = new FakeUnitOfWork();
        var emailSender = new ThrowingEmailSender();
        var handler = new CreateUserCommandHandler(
            repository,
            new FakePasswordService(),
            new FakeConfirmationIssuer(),
            emailSender);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateUserCommand(new CreateUserRequest("Tiago", "tiago@example.com", "Password123")),
            TestContext.Current.CancellationToken));

        Assert.Single(repository.UsersData);
        Assert.Single(repository.UserTokensData);
    }

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

    private sealed class ThrowingEmailSender : IEmailSender
    {
        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated provider failure.");
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
        public bool NeedsRehash(string passwordHash) => false;
    }

}
