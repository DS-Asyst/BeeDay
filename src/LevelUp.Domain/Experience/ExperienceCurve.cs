using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Experience;

public static class ExperienceCurve
{
    public const long BaseExperiencePerLevel = 100;

    public static int GetLevel(long totalExperience)
    {
        ValidateTotalExperience(totalExperience);

        var low = 1;
        var high = int.MaxValue;
        while (low < high)
        {
            var middle = low + ((high - low + 1) / 2);
            if (GetTotalExperienceRequiredForLevelAsDecimal(middle) <= totalExperience)
            {
                low = middle;
            }
            else
            {
                high = middle - 1;
            }
        }

        return low;
    }

    public static long GetTotalExperienceRequiredForLevel(int level)
    {
        ValidateLevel(level);
        var required = GetTotalExperienceRequiredForLevelAsDecimal(level);
        if (required > long.MaxValue)
        {
            throw new DomainValidationException(nameof(level), "Level exceeds the supported experience range.");
        }

        return (long)required;
    }

    public static long GetExperienceRequiredToAdvance(int currentLevel)
    {
        ValidateLevel(currentLevel);
        var required = (decimal)BaseExperiencePerLevel * currentLevel;
        if (required > long.MaxValue)
        {
            throw new DomainValidationException(nameof(currentLevel), "Level exceeds the supported experience range.");
        }

        return (long)required;
    }

    private static decimal GetTotalExperienceRequiredForLevelAsDecimal(int level) =>
        (decimal)BaseExperiencePerLevel * (level - 1) * level / 2;

    private static void ValidateTotalExperience(long totalExperience)
    {
        if (totalExperience < 0)
        {
            throw new DomainValidationException(nameof(totalExperience), "Total experience cannot be negative.");
        }
    }

    private static void ValidateLevel(int level)
    {
        if (level < 1)
        {
            throw new DomainValidationException(nameof(level), "Level must be greater than zero.");
        }
    }
}
