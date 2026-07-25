using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
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
    [JsonInclude] public bool HasCompletedOnboarding { get; private set; }
    [JsonInclude] public bool IsEmailConfirmed { get; private set; }
    [JsonInclude] public DateTimeOffset? EmailConfirmedAtUtc { get; private set; }

    public static User Create(string name, string email, string? passwordHash = null) =>
        Create(name, email, passwordHash, DateTimeOffset.UtcNow);

    public static User Create(
        string name,
        string email,
        string? passwordHash,
        DateTimeOffset createdAtUtc)
    {
        if (createdAtUtc == default)
        {
            throw new DomainValidationException(nameof(createdAtUtc), "Account creation date is required.");
        }

        var user = new User
        {
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

        user.UpdateName(name);
        user.Email = EmailAddress.Create(email).Value;
        user.PasswordHash = (passwordHash ?? string.Empty).Trim();
        user.UpdatedAtUtc = createdAtUtc;
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
    public void ConfirmEmail(DateTimeOffset confirmedAtUtc)
    {
        if (IsEmailConfirmed)
        {
            return;
        }

        if (confirmedAtUtc < CreatedAtUtc)
        {
            throw new DomainValidationException(nameof(confirmedAtUtc), "Email confirmation cannot precede account creation.");
        }

        IsEmailConfirmed = true;
        EmailConfirmedAtUtc = confirmedAtUtc;
        UpdatedAtUtc = confirmedAtUtc;
    }

    public void RegisterLogin() { LastLoginAtUtc = DateTimeOffset.UtcNow; Touch(); }
    public void SetActive(bool active) { IsActive = active; Touch(); }
    public void CompleteOnboarding() { HasCompletedOnboarding = true; Touch(); }
    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
