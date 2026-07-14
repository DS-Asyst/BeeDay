using LevelUp.Domain.Character;

namespace LevelUp.Services;

public class ProgressionService
{
    public void AddExperience(
        ILevelProgress progress,
        decimal experienceEarned)
    {
        progress.Experience += experienceEarned;

        while (
            progress.Experience >=
            progress.ExperienceToNextLevel)
        {
            decimal requiredExperience =
                progress.ExperienceToNextLevel;

            progress.Experience -= requiredExperience;
            progress.Level++;
        }
    }
}