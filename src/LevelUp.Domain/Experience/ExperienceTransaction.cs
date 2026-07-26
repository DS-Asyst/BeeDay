using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Experience;

public sealed class ExperienceTransaction : Entity
{
    [JsonInclude]
    public long Amount { get; private set; }

    [JsonInclude]
    public ExperienceSource Source { get; private set; } = ExperienceSource.Create(LevelUp.Domain.Enums.ExperienceSourceType.System);

    [JsonIgnore]
    public ExperienceSourceType SourceType => Source.Type;

    [JsonIgnore]
    public Guid? SourceId => Source.ReferenceId;

    [JsonInclude]
    public ExperienceRewardType RewardType { get; private set; }

    [JsonInclude]
    public DateTimeOffset GrantedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
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
