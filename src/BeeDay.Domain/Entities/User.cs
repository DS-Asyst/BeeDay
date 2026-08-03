using BeeDay.Domain.Abstractions;
using BeeDay.Domain.Common;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.Experience;
using BeeDay.Domain.ValueObjects;

namespace BeeDay.Domain.Entities;

public sealed class User : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserLanguage Language { get; private set; } = UserLanguage.English;
    public UserTheme Theme { get; private set; } = UserTheme.System;
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAtUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool HasCompletedOnboarding { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public DateTimeOffset? EmailConfirmedAtUtc { get; private set; }
    public string Nickname { get; private set; } = string.Empty;
    public string Avatar { get; private set; } = string.Empty;
    public UserExperience Experience { get; private set; } = UserExperience.Create();

    /// <summary>
    /// Incremented whenever previously-issued session cookies must stop being honored (password
    /// change, password reset, account deactivation). The active session's claim is compared
    /// against this value on every request; a mismatch signs the session out.
    /// </summary>
    public int SessionVersion { get; private set; } = 1;

    public bool HasProfile => !string.IsNullOrEmpty(Nickname);

    /// <summary>
    /// Presentation-facing view of this User (nickname, name, avatar, preferences, progress),
    /// carrying no authentication state. Prefer this over reading the fields above directly
    /// for anything Profile-facing. See <see cref="Entities.Profile"/> remarks for why the
    /// underlying fields still live on User rather than a separately-persisted aggregate.
    /// </summary>
    public Profile Profile => new(Nickname, Name, Avatar, Language, Theme, Experience);

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

    public void SetActive(bool active)
    {
        IsActive = active;
        if (!active)
        {
            InvalidateSessions();
            return;
        }

        Touch();
    }

    public void CompleteOnboarding() { HasCompletedOnboarding = true; Touch(); }

    /// <summary>
    /// Revokes every session issued before this call by advancing <see cref="SessionVersion"/>.
    /// Call explicitly after a genuine security-relevant change (password change, password
    /// reset, deactivation) — not from <see cref="SetPasswordHash"/> itself, since that method
    /// is also used for transparent hash-format upgrades on login, which must not sign out the
    /// very session being created.
    /// </summary>
    public void InvalidateSessions() { SessionVersion++; Touch(); }

    public void CompleteProfile(string nickname, string? avatar)
    {
        if (HasProfile)
        {
            throw new InvalidDomainStateException("A User can only complete their profile once.");
        }

        Nickname = BeeDay.Domain.ValueObjects.Nickname.Create(nickname).Value;
        Avatar = (avatar ?? string.Empty).Trim();
        Touch();
    }

    public void UpdateAvatar(string? avatar)
    {
        Avatar = (avatar ?? string.Empty).Trim();
        Touch();
    }

    public ExperienceEntry AddExperience(
        ExperienceReward reward,
        ExperienceSource source,
        DateTimeOffset? occurredAtUtc = null) =>
        AddExperience(reward, source, ExperienceRewardType.Completion, occurredAtUtc);

    public ExperienceEntry AddExperience(
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? occurredAtUtc = null)
    {
        var entry = Experience.Add(Id, reward, source, rewardType, occurredAtUtc);
        UpdatedAtUtc = entry.OccurredAtUtc;
        return entry;
    }

    public ExperienceEntry? TryAddExperience(
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? grantedAtUtc = null)
    {
        var entry = Experience.TryAdd(Id, reward, source, rewardType, grantedAtUtc);
        if (entry is not null)
        {
            UpdatedAtUtc = entry.GrantedAtUtc;
        }

        return entry;
    }

    internal void EnsureExperienceState()
    {
        Experience ??= UserExperience.Create();
        Experience.EnsureValidState();
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
