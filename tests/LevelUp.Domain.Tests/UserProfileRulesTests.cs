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
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        user.CompleteProfile("tiago", "old-avatar");

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
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        user.CompleteProfile("tiago", null);

        Assert.Throws<InvalidDomainStateException>(() => user.CompleteProfile("othernick", null));
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

    [Fact]
    public void Profile_ReflectsUnderlyingUserPresentationData()
    {
        var user = User.Create("Tiago", "tiago@levelup.invalid");
        user.CompleteProfile("tiago", "avatar-key");
        user.UpdatePreferences(UserLanguage.Portuguese, UserTheme.Dark);

        var profile = user.Profile;

        Assert.Equal(user.Nickname, profile.Nickname);
        Assert.Equal(user.Name, profile.Name);
        Assert.Equal(user.Avatar, profile.Avatar);
        Assert.Equal(user.Language, profile.Language);
        Assert.Equal(user.Theme, profile.Theme);
        Assert.Same(user.Experience, profile.Experience);
        Assert.True(profile.IsComplete);
    }

    [Fact]
    public void Profile_IsIncomplete_BeforeProfileCreation()
    {
        var user = User.Create("Tiago", "tiago@levelup.invalid");

        Assert.False(user.Profile.IsComplete);
    }
}
