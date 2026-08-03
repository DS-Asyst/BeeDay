namespace BeeDay.Domain.Experience;

public interface IExperienceCurve
{
    public long BaseExperience { get; }

    public int? MaximumLevel { get; }

    public int GetLevel(long totalExperience);

    public long GetTotalExperienceRequiredForLevel(int level);

    public long GetExperienceRequiredToAdvance(int currentLevel);
}
