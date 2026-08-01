using LevelUp.Application.Common.Contracts;
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
        var repository = new TestRepository();
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

        var user = Assert.Single(repository.Data.Users);
        Assert.Equal(userId, user.Id);
        Assert.True(user.HasProfile);
        Assert.Equal("tiago", user.Nickname);
        Assert.Equal("hash:Password123", user.PasswordHash);
        Assert.Single(repository.Data.UserTokens);
        Assert.Single(emailSender.Messages);
    }


    private sealed class FakeConfirmationIssuer : IEmailConfirmationIssuer
    {
        public EmailMessage Issue(LevelUpData data, User user)
        {
            var now = user.CreatedAtUtc;
            data.AddUserToken(UserToken.Create(
                user.Id,
                UserTokenType.EmailConfirmation,
                "hash:confirmation",
                now,
                now.AddHours(24)));
            return new EmailMessage(user.Email, "Confirm", "Body");
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
