using BeeDay.Domain.Enums;

namespace BeeDay.Application.Common.Experience;

public interface IExperienceRewardPolicy
{
    public long GetReward(ExperienceSourceType sourceType);
}
