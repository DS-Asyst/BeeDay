using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.Experience;

namespace BeeDay.Application.Common.Experience;

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
