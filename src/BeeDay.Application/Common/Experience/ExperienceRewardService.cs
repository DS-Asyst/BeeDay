using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using LevelUp.Domain.Experience;

namespace LevelUp.Application.Common.Experience;

public sealed class ExperienceRewardService(IExperienceRewardPolicy? policy = null) : IExperienceRewardService
{
    private readonly IExperienceRewardPolicy _policy = policy ?? new ExperienceRewardPolicy();

    public ExperienceEntry? Grant(
        User user,
        ExperienceSourceType sourceType,
        Guid sourceId,
        ExperienceRewardType rewardType,
        string? description = null,
        DateTimeOffset? grantedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (sourceId == Guid.Empty)
        {
            throw new DomainValidationException(nameof(sourceId), "Experience source identifier is required.");
        }

        if (rewardType != ExperienceRewardType.Completion)
        {
            throw new DomainValidationException(nameof(rewardType), "Unsupported experience reward type.");
        }

        return user.TryAddExperience(
            ExperienceReward.Create(_policy.GetReward(sourceType)),
            ExperienceSource.Create(sourceType, sourceId, description),
            rewardType,
            grantedAtUtc);
    }
}
