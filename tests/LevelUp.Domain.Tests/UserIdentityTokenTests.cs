using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class UserIdentityTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void User_StartsWithUnconfirmedEmail()
    {
        var user = User.Create("Tiago", "tiago@levelup.invalid");

        Assert.False(user.IsEmailConfirmed);
        Assert.Null(user.EmailConfirmedAtUtc);
    }

    [Fact]
    public void ConfirmEmail_ChangesConfirmationState()
    {
        var user = User.Create("Tiago", "tiago@levelup.invalid");

        user.ConfirmEmail(user.CreatedAtUtc.AddMinutes(1));

        Assert.True(user.IsEmailConfirmed);
        Assert.Equal(user.CreatedAtUtc.AddMinutes(1), user.EmailConfirmedAtUtc);
    }

    [Fact]
    public void ConfirmEmail_IsIdempotent()
    {
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        var firstConfirmation = user.CreatedAtUtc.AddMinutes(1);

        user.ConfirmEmail(firstConfirmation);
        user.ConfirmEmail(firstConfirmation.AddMinutes(1));

        Assert.Equal(firstConfirmation, user.EmailConfirmedAtUtc);
    }

    [Fact]
    public void Token_Create_RejectsInvalidExpiration()
    {
        Assert.Throws<DomainValidationException>(() => UserToken.Create(
            Guid.NewGuid(),
            UserTokenType.EmailConfirmation,
            "hash",
            Now,
            Now));
    }

    [Fact]
    public void ExpiredToken_CannotBeUsed()
    {
        var token = CreateToken(UserTokenType.EmailConfirmation, Now.AddMinutes(5));

        Assert.Throws<InvalidDomainStateException>(() =>
            token.MarkAsUsed(UserTokenType.EmailConfirmation, Now.AddMinutes(5)));
    }

    [Fact]
    public void UsedToken_CannotBeReused()
    {
        var token = CreateToken(UserTokenType.EmailConfirmation, Now.AddMinutes(5));
        token.MarkAsUsed(UserTokenType.EmailConfirmation, Now.AddMinutes(1));

        Assert.Throws<InvalidDomainStateException>(() =>
            token.MarkAsUsed(UserTokenType.EmailConfirmation, Now.AddMinutes(2)));
    }

    [Fact]
    public void PasswordResetToken_CannotConfirmEmail()
    {
        var token = CreateToken(UserTokenType.PasswordReset, Now.AddMinutes(5));

        Assert.Throws<InvalidDomainStateException>(() =>
            token.MarkAsUsed(UserTokenType.EmailConfirmation, Now.AddMinutes(1)));
    }

    [Fact]
    public void EmailConfirmationToken_CannotResetPassword()
    {
        var token = CreateToken(UserTokenType.EmailConfirmation, Now.AddMinutes(5));

        Assert.Throws<InvalidDomainStateException>(() =>
            token.MarkAsUsed(UserTokenType.PasswordReset, Now.AddMinutes(1)));
    }

    [Fact]
    public void RevokedToken_CannotBeUsed()
    {
        var token = CreateToken(UserTokenType.PasswordReset, Now.AddMinutes(5));
        token.Revoke(Now.AddMinutes(1));

        Assert.Throws<InvalidDomainStateException>(() =>
            token.MarkAsUsed(UserTokenType.PasswordReset, Now.AddMinutes(2)));
    }


    [Fact]
    public void LevelUpData_AddUserToken_RequiresExistingOwner()
    {
        var data = new LevelUpData();
        var token = CreateToken(UserTokenType.EmailConfirmation, Now.AddMinutes(5));

        Assert.Throws<InvalidDomainStateException>(() => data.AddUserToken(token));
    }

    [Fact]
    public void LevelUpData_RevokeActiveUserTokens_RevokesMatchingTypeOnly()
    {
        var data = new LevelUpData();
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        data.AddUser(user);
        var confirmation = UserToken.Create(user.Id, UserTokenType.EmailConfirmation, "confirmation", Now, Now.AddMinutes(5));
        var reset = UserToken.Create(user.Id, UserTokenType.PasswordReset, "reset", Now, Now.AddMinutes(5));
        data.AddUserToken(confirmation);
        data.AddUserToken(reset);

        data.RevokeActiveUserTokens(user.Id, UserTokenType.PasswordReset, Now.AddMinutes(1));

        Assert.False(confirmation.IsRevoked);
        Assert.True(reset.IsRevoked);
    }

    private static UserToken CreateToken(UserTokenType type, DateTimeOffset expiresAtUtc) =>
        UserToken.Create(Guid.NewGuid(), type, "token-hash", Now, expiresAtUtc);
}
