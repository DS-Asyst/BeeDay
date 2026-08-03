using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Experience;

public sealed class ExperienceTransaction : Entity
{
    public long Amount { get; private set; }

    public ExperienceSource Source { get; private set; } = ExperienceSource.Create(LevelUp.Domain.Enums.ExperienceSourceType.System);

    public ExperienceSourceType SourceType => Source.Type;

    public Guid? SourceId => Source.ReferenceId;

    public ExperienceRewardType RewardType { get; private set; }

    public DateTimeOffset GrantedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset OccurredAtUtc => GrantedAtUtc;

    public static ExperienceTransaction Create(
        ExperienceReward reward,
        ExperienceSource source,
        ExperienceRewardType rewardType,
        DateTimeOffset? grantedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ExperienceTransaction
        {
            Amount = reward.Amount,
            Source = source,
            RewardType = rewardType,
            GrantedAtUtc = grantedAtUtc ?? DateTimeOffset.UtcNow
        };
    }
}
