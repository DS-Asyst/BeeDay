using BeeDay.Domain.Abstractions;
using BeeDay.Domain.Enums;

namespace BeeDay.Domain.Experience;

public sealed class ExperienceEntry : Entity
{
    public Guid UserId { get; private set; }

    public long Amount { get; private set; }

    public ExperienceSource Source { get; private set; } = ExperienceSource.Create(ExperienceSourceType.System);

    public ExperienceSourceType SourceType => Source.Type;

    public Guid? SourceId => Source.ReferenceId;

    public ExperienceRewardType RewardType { get; private set; }

    public long ExperienceBefore { get; private set; }

    public long ExperienceAfter { get; private set; }

    public int LevelBefore { get; private set; }

    public int LevelAfter { get; private set; }

    public DateTimeOffset GrantedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

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
