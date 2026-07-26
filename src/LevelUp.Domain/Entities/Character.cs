using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Domain.Entities;

public sealed class Character : Entity
{
    [JsonInclude] public Guid UserId { get; private set; }
    [JsonInclude] public string Nickname { get; private set; } = string.Empty;
    [JsonInclude] public CharacterClass Class { get; private set; } = CharacterClass.Warrior;
    [JsonInclude] public string Avatar { get; private set; } = string.Empty;
    [JsonInclude] public CharacterExperience Experience { get; private set; } = CharacterExperience.Create();
    [JsonInclude] public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    [JsonInclude] public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public static Character Create(Guid userId, string nickname, CharacterClass characterClass, string? avatar = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }
        return new Character
        {
            UserId = userId,
            Nickname = CharacterNickname.Create(nickname).Value,
            Class = EnumValidation.Defined(characterClass, nameof(characterClass)),
            Avatar = (avatar ?? string.Empty).Trim()
        };
    }

    public ExperienceTransaction AddExperience(
        ExperienceReward reward,
        ExperienceSource source,
        DateTimeOffset? occurredAtUtc = null) =>
        AddExperience(reward, source, ExperienceRewardType.Completion, occurredAtUtc);

    public ExperienceTransaction AddExperience(
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? occurredAtUtc = null)
    {
        var transaction = Experience.Add(reward, source, rewardType, occurredAtUtc);
        UpdatedAtUtc = transaction.OccurredAtUtc;
        return transaction;
    }


    public ExperienceTransaction? TryAddExperience(
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? grantedAtUtc = null)
    {
        var transaction = Experience.TryAdd(reward, source, rewardType, grantedAtUtc);
        if (transaction is not null)
        {
            UpdatedAtUtc = transaction.GrantedAtUtc;
        }

        return transaction;
    }

    internal void EnsureExperienceState()
    {
        Experience ??= CharacterExperience.Create();
        Experience.EnsureValidState();
    }

    public void UpdateAvatar(string? avatar)
    {
        Avatar = (avatar ?? string.Empty).Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
