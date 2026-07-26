using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using LevelUp.Domain.Experience;

namespace LevelUp.Application.Common.Experience;

public sealed class ExperienceRewardService(IExperienceRewardPolicy? policy = null) : IExperienceRewardService
{
    private readonly IExperienceRewardPolicy _policy = policy ?? new ExperienceRewardPolicy();

    public ExperienceEntry? Grant(
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

        if (rewardType != ExperienceRewardType.Completion)
        {
            throw new DomainValidationException(nameof(rewardType), "Unsupported experience reward type.");
        }

        Character character = data.FindCharacterForUser(userId)
            ?? throw new InvalidDomainStateException("A Character is required before experience can be granted.");

        return character.TryAddExperience(
            ExperienceReward.Create(_policy.GetReward(sourceType)),
            ExperienceSource.Create(sourceType, sourceId, description),
            rewardType,
            grantedAtUtc);
    }
}
