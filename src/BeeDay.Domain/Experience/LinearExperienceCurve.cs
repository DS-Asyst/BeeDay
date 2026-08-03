using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Experience;

public sealed class LinearExperienceCurve : IExperienceCurve
{
    public LinearExperienceCurve(long baseExperience = 100, int? maximumLevel = null)
    {
        if (baseExperience <= 0)
        {
            throw new DomainValidationException(nameof(baseExperience), "Base experience must be greater than zero.");
        }

        if (maximumLevel is < 1)
        {
            throw new DomainValidationException(nameof(maximumLevel), "Maximum level must be greater than zero.");
        }

        BaseExperience = baseExperience;
        MaximumLevel = maximumLevel;
    }

    public long BaseExperience { get; }

    public int? MaximumLevel { get; }

    public int GetLevel(long totalExperience)
    {
        ValidateTotalExperience(totalExperience);

        var low = 1;
        var high = MaximumLevel ?? int.MaxValue;
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

    public long GetTotalExperienceRequiredForLevel(int level)
    {
        ValidateLevel(level);
        var required = GetTotalExperienceRequiredForLevelAsDecimal(level);
        if (required > long.MaxValue)
        {
            throw new DomainValidationException(nameof(level), "Level exceeds the supported experience range.");
        }

        return (long)required;
    }

    public long GetExperienceRequiredToAdvance(int currentLevel)
    {
        ValidateLevel(currentLevel);

        if (MaximumLevel.HasValue && currentLevel >= MaximumLevel.Value)
        {
            return 0;
        }

        var required = (decimal)BaseExperience * currentLevel;
        if (required > long.MaxValue)
        {
            throw new DomainValidationException(nameof(currentLevel), "Level exceeds the supported experience range.");
        }

        return (long)required;
    }

    private decimal GetTotalExperienceRequiredForLevelAsDecimal(int level) =>
        (decimal)BaseExperience * (level - 1) * level / 2;

    private static void ValidateTotalExperience(long totalExperience)
    {
        if (totalExperience < 0)
        {
            throw new DomainValidationException(nameof(totalExperience), "Total experience cannot be negative.");
        }
    }

    private void ValidateLevel(int level)
    {
        if (level < 1)
        {
            throw new DomainValidationException(nameof(level), "Level must be greater than zero.");
        }

        if (MaximumLevel.HasValue && level > MaximumLevel.Value)
        {
            throw new DomainValidationException(nameof(level), "Level exceeds the configured maximum level.");
        }
    }
}
