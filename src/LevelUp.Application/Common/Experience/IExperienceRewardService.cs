using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;

namespace LevelUp.Application.Common.Experience;

public interface IExperienceRewardService
{
    public ExperienceTransaction? Grant(
        LevelUpData data,
        Guid userId,
        ExperienceSourceType sourceType,
        Guid sourceId,
        ExperienceRewardType rewardType,
        string? description = null,
        DateTimeOffset? grantedAtUtc = null);
}
