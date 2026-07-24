using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class UserAndCharacterRulesTests
{
    [Fact]
    public void Character_UpdateAvatar_PreservesImmutableNickname()
    {
        var character = Character.Create(Guid.NewGuid(), "tiago", CharacterClass.Warrior, "old-avatar");

        character.UpdateAvatar("new-avatar");

        Assert.Equal("tiago", character.Nickname);
        Assert.Equal("new-avatar", character.Avatar);
    }

    [Fact]
    public void Character_DoesNotExposeNicknameMutationOperation()
    {
        var publicMutationMethods = typeof(Character)
            .GetMethods()
            .Where(method => method.IsPublic && !method.IsStatic && !method.IsSpecialName)
            .Where(method => method.Name.Contains("Nickname", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Empty(publicMutationMethods);
        Assert.NotNull(typeof(Character).GetProperty(nameof(Character.Nickname)));
        Assert.False(typeof(Character).GetProperty(nameof(Character.Nickname))!.SetMethod?.IsPublic ?? false);
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
