using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class UserProfileRulesTests
{
    [Fact]
    public void User_UpdateAvatar_PreservesImmutableNickname()
    {
        var data = new LevelUpData();
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        data.AddUser(user);
        data.CompleteUserProfile(user.Id, "tiago", "old-avatar");

        user.UpdateAvatar("new-avatar");

        Assert.Equal("tiago", user.Nickname);
        Assert.Equal("new-avatar", user.Avatar);
    }

    [Fact]
    public void User_DoesNotExposePublicNicknameMutationOperation()
    {
        var publicMutationMethods = typeof(User)
            .GetMethods()
            .Where(method => method.IsPublic && !method.IsStatic && !method.IsSpecialName)
            .Where(method => method.Name.Contains("Nickname", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Empty(publicMutationMethods);
        Assert.NotNull(typeof(User).GetProperty(nameof(User.Nickname)));
        Assert.False(typeof(User).GetProperty(nameof(User.Nickname))!.SetMethod?.IsPublic ?? false);
    }

    [Fact]
    public void CompleteUserProfile_RejectsCompletingProfileTwice()
    {
        var data = new LevelUpData();
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        data.AddUser(user);
        data.CompleteUserProfile(user.Id, "tiago");

        Assert.Throws<InvalidDomainStateException>(() => data.CompleteUserProfile(user.Id, "othernick"));
    }

    [Fact]
    public void CompleteUserProfile_RejectsDuplicateNickname()
    {
        var data = new LevelUpData();
        var first = User.Create("First", "first@levelup.invalid");
        var second = User.Create("Second", "second@levelup.invalid");
        data.AddUser(first);
        data.AddUser(second);
        data.CompleteUserProfile(first.Id, "tiago");

        Assert.Throws<InvalidDomainStateException>(() => data.CompleteUserProfile(second.Id, "tiago"));
    }

    [Fact]
    public void User_UpdateAccount_ChangesProfileData()
    {
        var user = User.Create("Old Name", "old@levelup.invalid");

        user.UpdateAccount("New Name", "new@levelup.invalid");

        Assert.Equal("New Name", user.Name);
        Assert.Equal("new@levelup.invalid", user.Email);
    }

    [Fact]
    public void User_UpdatePreferences_ChangesLanguageAndTheme()
    {
        var user = User.Create("Tiago", "tiago@levelup.invalid");

        user.UpdatePreferences(UserLanguage.Portuguese, UserTheme.Dark);

        Assert.Equal(UserLanguage.Portuguese, user.Language);
        Assert.Equal(UserTheme.Dark, user.Theme);
    }

    [Fact]
    public void CompleteOnboarding_IsPersistedOnUser()
    {
        var user = User.Create("Test User", "test@levelup.invalid");

        user.CompleteOnboarding();

        Assert.True(user.HasCompletedOnboarding);
    }
}
