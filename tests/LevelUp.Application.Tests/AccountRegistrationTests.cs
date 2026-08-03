using LevelUp.Application.Common.Identity;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Handlers;
using LevelUp.Application.Features.Users.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Tests;

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

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
        public bool NeedsRehash(string passwordHash) => false;
    }

}
