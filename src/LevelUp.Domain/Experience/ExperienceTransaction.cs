using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;

namespace LevelUp.Domain.Experience;

public sealed class ExperienceTransaction : Entity
{
    [JsonInclude]
    public long Amount { get; private set; }

    [JsonInclude]
    public ExperienceSource Source { get; private set; } = ExperienceSource.Create(LevelUp.Domain.Enums.ExperienceSourceType.System);

    [JsonInclude]
    public DateTimeOffset OccurredAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public static ExperienceTransaction Create(
        ExperienceReward reward,
        ExperienceSource source,
        DateTimeOffset? occurredAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ExperienceTransaction
        {
            Amount = reward.Amount,
            Source = source,
            OccurredAtUtc = occurredAtUtc ?? DateTimeOffset.UtcNow
        };
    }
}
