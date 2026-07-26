using LevelUp.Domain.Experience;

namespace LevelUp.Web.Components.Features.Character.Models;

public sealed record CharacterExperienceViewModel(
    int Level,
    long CurrentExperience,
    long RequiredExperience,
    long RemainingExperience)
{
    public double ProgressPercentage => RequiredExperience <= 0
        ? 100d
        : Math.Clamp(CurrentExperience * 100d / RequiredExperience, 0d, 100d);

    public static CharacterExperienceViewModel From(CharacterExperience experience)
    {
        ArgumentNullException.ThrowIfNull(experience);

        return new CharacterExperienceViewModel(
            experience.CurrentLevel,
            experience.CurrentLevelExperience,
            experience.ExperienceRequiredForCurrentLevel,
            experience.ExperienceForNextLevel);
    }
}
