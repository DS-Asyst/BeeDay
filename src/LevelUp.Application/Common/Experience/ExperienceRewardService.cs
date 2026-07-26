using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using LevelUp.Domain.Experience;

namespace LevelUp.Application.Common.Experience;

public sealed class ExperienceRewardService : IExperienceRewardService
{
    public ExperienceTransaction? Grant(
        LevelUpData data,
        Guid userId,
        ExperienceSourceType sourceType,
        Guid sourceId,
        ExperienceRewardType rewardType,
        string? description = null,
        DateTimeOffset? grantedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (sourceId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(sourceId), "Experience source identifier is required.");
        }

        var character = data.FindCharacterForUser(userId)
            ?? throw new InvalidDomainStateException("A Character is required before experience can be granted.");

        var amount = GetRewardAmount(sourceType, rewardType);
        return character.TryAddExperience(
            ExperienceReward.Create(amount),
            ExperienceSource.Create(sourceType, sourceId, description),
            rewardType,
            grantedAtUtc);
    }

    private static long GetRewardAmount(ExperienceSourceType sourceType, ExperienceRewardType rewardType)
    {
        if (rewardType != ExperienceRewardType.Completion)
        {
            throw new DomainValidationException(nameof(rewardType), "Unsupported experience reward type.");
        }

        return sourceType switch
        {
            ExperienceSourceType.Habit => 10,
            ExperienceSourceType.Task => 20,
            ExperienceSourceType.Todo => 25,
            ExperienceSourceType.Project => 50,
            ExperienceSourceType.Reading => 10,
            _ => throw new DomainValidationException(nameof(sourceType), "This source cannot use the automatic reward pipeline.")
        };
    }
}
