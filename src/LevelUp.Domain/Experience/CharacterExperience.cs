using System.Text.Json.Serialization;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Experience;

public sealed class CharacterExperience
{
    [JsonInclude]
    public long TotalExperience { get; private set; }

    [JsonInclude]
    [JsonPropertyName("Transactions")]
    public IReadOnlyList<ExperienceEntry> Entries { get; private set; } = [];

    [JsonIgnore]
    public int CurrentLevel => ExperienceCurve.GetLevel(TotalExperience);

    [JsonIgnore]
    public long CurrentLevelExperience => TotalExperience - ExperienceCurve.GetTotalExperienceRequiredForLevel(CurrentLevel);

    [JsonIgnore]
    public long ExperienceRequiredForCurrentLevel => ExperienceCurve.GetExperienceRequiredToAdvance(CurrentLevel);

    [JsonIgnore]
    public long ExperienceForNextLevel => ExperienceRequiredForCurrentLevel - CurrentLevelExperience;

    public static CharacterExperience Create() => new();

    public ExperienceEntry Add(
        ExperienceReward reward,
        ExperienceSource source,
        DateTimeOffset? occurredAtUtc = null) =>
        Add(Guid.NewGuid(), reward, source, ExperienceRewardType.Completion, occurredAtUtc);

    public ExperienceEntry Add(
        Guid characterId,
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType = ExperienceRewardType.Completion,
        DateTimeOffset? occurredAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (reward.Amount <= 0)
        {
            throw new DomainValidationException(nameof(reward), "Experience reward must be greater than zero.");
        }

        if (reward.Amount > long.MaxValue - TotalExperience)
        {
            throw new InvalidDomainStateException("Total experience exceeds the supported range.");
        }

        long experienceBefore = TotalExperience;
        int levelBefore = ExperienceCurve.GetLevel(experienceBefore);
        long experienceAfter = checked(experienceBefore + reward.Amount);
        int levelAfter = ExperienceCurve.GetLevel(experienceAfter);
        ExperienceEntry entry = ExperienceEntry.Create(
            characterId,
            reward,
            source,
            rewardType,
            experienceBefore,
            experienceAfter,
            levelBefore,
            levelAfter,
            occurredAtUtc);

        TotalExperience = experienceAfter;
        Entries = [.. Entries, entry];

        return entry;
    }

    public ExperienceEntry? TryAdd(
        Guid characterId,
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? grantedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.ReferenceId is not Guid sourceId)
        {
            throw new DomainValidationException(nameof(source), "Automatic experience rewards require a source identifier.");
        }

        bool duplicate = Entries.Any(entry =>
            entry.CharacterId == characterId &&
            entry.Source.Type == source.Type &&
            entry.Source.ReferenceId == sourceId &&
            entry.RewardType == rewardType);

        return duplicate
            ? null
            : Add(characterId, reward, source, rewardType, grantedAtUtc);
    }

    internal void EnsureValidState()
    {
        Entries ??= [];

        if (TotalExperience < 0)
        {
            throw new InvalidDomainStateException("Character total experience cannot be negative.");
        }

        if (Entries.Any(entry => entry.Amount <= 0))
        {
            throw new InvalidDomainStateException("Experience history cannot contain non-positive rewards.");
        }

        bool duplicate = Entries
            .Where(entry => entry.Source.ReferenceId.HasValue && entry.Source.Type != ExperienceSourceType.Habit)
            .GroupBy(entry => new
            {
                entry.CharacterId,
                entry.Source.Type,
                entry.Source.ReferenceId,
                entry.RewardType,
            })
            .Any(group => group.Count() > 1);

        if (duplicate)
        {
            throw new InvalidDomainStateException("Experience history contains a duplicate reward key.");
        }
    }
}
