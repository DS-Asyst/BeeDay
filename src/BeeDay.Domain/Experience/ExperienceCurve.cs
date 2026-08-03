namespace BeeDay.Domain.Experience;

public static class ExperienceCurve
{
    public const long BaseExperiencePerLevel = 100;

    public static IExperienceCurve Default { get; } = new LinearExperienceCurve(BaseExperiencePerLevel);

    public static int GetLevel(long totalExperience) => Default.GetLevel(totalExperience);

    public static long GetTotalExperienceRequiredForLevel(int level) =>
        Default.GetTotalExperienceRequiredForLevel(level);

    public static long GetExperienceRequiredToAdvance(int currentLevel) =>
        Default.GetExperienceRequiredToAdvance(currentLevel);
}
