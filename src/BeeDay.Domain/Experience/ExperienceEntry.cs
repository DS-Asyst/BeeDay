using BeeDay.Domain.Abstractions;
using BeeDay.Domain.Common;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.Experience;

public sealed class ExperienceEntry : Entity
{
    private ExperienceEntry() { }

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

        if (reward.Amount <= 0)
        {
            throw new DomainValidationException(nameof(reward), "Experience reward must be greater than zero.");
        }

        EnumValidation.Defined(rewardType, nameof(rewardType));

        if (experienceBefore < 0)
        {
            throw new DomainValidationException(nameof(experienceBefore), "Experience before the reward cannot be negative.");
        }

        long expectedExperienceAfter;
        try
        {
            expectedExperienceAfter = checked(experienceBefore + reward.Amount);
        }
        catch (OverflowException)
        {
            throw new DomainValidationException(nameof(experienceAfter), "Experience after the reward exceeds the supported range.");
        }

        if (experienceAfter != expectedExperienceAfter)
        {
            throw new DomainValidationException(nameof(experienceAfter), "Experience after the reward must equal the previous total plus the reward.");
        }

        if (levelBefore != ExperienceCurve.GetLevel(experienceBefore))
        {
            throw new DomainValidationException(nameof(levelBefore), "Level before the reward does not match the experience curve.");
        }

        if (levelAfter != ExperienceCurve.GetLevel(experienceAfter))
        {
            throw new DomainValidationException(nameof(levelAfter), "Level after the reward does not match the experience curve.");
        }

        if (grantedAtUtc.HasValue && grantedAtUtc.Value == default)
        {
            throw new DomainValidationException(nameof(grantedAtUtc), "Experience grant time is required when provided.");
        }

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
