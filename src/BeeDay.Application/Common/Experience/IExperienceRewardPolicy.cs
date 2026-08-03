using LevelUp.Domain.Enums;

namespace LevelUp.Application.Common.Experience;

public interface IExperienceRewardPolicy
{
    public long GetReward(ExperienceSourceType sourceType);
}
