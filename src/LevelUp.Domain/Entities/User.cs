using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Domain.Entities;

public sealed class User : Entity
{
    [JsonInclude] public string Name { get; private set; } = string.Empty;
    [JsonInclude] public string Email { get; private set; } = string.Empty;
    [JsonInclude] public string PasswordHash { get; private set; } = string.Empty;
    [JsonInclude] public UserLanguage Language { get; private set; } = UserLanguage.English;
    [JsonInclude] public UserTheme Theme { get; private set; } = UserTheme.System;
    [JsonInclude] public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    [JsonInclude] public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    [JsonInclude] public DateTimeOffset? LastLoginAtUtc { get; private set; }
    [JsonInclude] public bool IsActive { get; private set; } = true;

    public static User Create(string name, string email, string? passwordHash = null)
    {
        var user = new User();
        user.UpdateName(name);
        user.Email = EmailAddress.Create(email).Value;
        user.PasswordHash = (passwordHash ?? string.Empty).Trim();
        return user;
    }

    public void UpdateName(string name) { Name = UserName.Create(name).Value; Touch(); }
    public void UpdateAccount(string name, string email)
    {
        Name = UserName.Create(name).Value;
        Email = EmailAddress.Create(email).Value;
        Touch();
    }
    public void UpdatePreferences(UserLanguage language, UserTheme theme)
    {
        Language = EnumValidation.Defined(language, nameof(language));
        Theme = EnumValidation.Defined(theme, nameof(theme));
        Touch();
    }
    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash.Trim();
        Touch();
    }
    public void RegisterLogin() { LastLoginAtUtc = DateTimeOffset.UtcNow; Touch(); }
    public void SetActive(bool active) { IsActive = active; Touch(); }
    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
