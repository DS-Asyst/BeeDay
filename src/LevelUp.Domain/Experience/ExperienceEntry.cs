using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Experience;

public sealed class ExperienceEntry : Entity
{
    [JsonInclude]
    public Guid UserId { get; private set; }

    [JsonInclude]
    public long Amount { get; private set; }

    [JsonInclude]
    public ExperienceSource Source { get; private set; } = ExperienceSource.Create(ExperienceSourceType.System);

    [JsonIgnore]
    public ExperienceSourceType SourceType => Source.Type;

    [JsonIgnore]
    public Guid? SourceId => Source.ReferenceId;

    [JsonInclude]
    public ExperienceRewardType RewardType { get; private set; }

    [JsonInclude]
    public long ExperienceBefore { get; private set; }

    [JsonInclude]
    public long ExperienceAfter { get; private set; }

    [JsonInclude]
    public int LevelBefore { get; private set; }

    [JsonInclude]
    public int LevelAfter { get; private set; }

    [JsonInclude]
    public DateTimeOffset GrantedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public DateTimeOffset OccurredAtUtc => GrantedAtUtc;

    public static ExperienceEntry Create(
        Guid userId,
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        long experienceBefore,
        long experienceAfter,
        int levelBefore,
        int levelAfter,
        DateTimeOffset? grantedAtUtc = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(source);

        return new ExperienceEntry
        {
            UserId = userId,
            Amount = reward.Amount,
            Source = source,
            RewardType = rewardType,
            ExperienceBefore = experienceBefore,
            ExperienceAfter = experienceAfter,
            LevelBefore = levelBefore,
            LevelAfter = levelAfter,
            GrantedAtUtc = grantedAtUtc ?? DateTimeOffset.UtcNow,
        };
    }
}
