using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Common.Experience;

public sealed class ExperienceRewardPolicy : IExperienceRewardPolicy
{
    public const long PositiveHabitReward = 1;
    public const long TaskCompletionReward = 5;
    public const long TodoCompletionReward = 7;
    public const long ProjectCompletionReward = 20;

    public long GetReward(ExperienceSourceType sourceType) => sourceType switch
    {
        ExperienceSourceType.Habit => PositiveHabitReward,
        ExperienceSourceType.Task => TaskCompletionReward,
        ExperienceSourceType.Todo => TodoCompletionReward,
        ExperienceSourceType.Project => ProjectCompletionReward,
        _ => throw new DomainValidationException(
            nameof(sourceType),
            "This source cannot use the automatic reward policy."),
    };
}
