using System.Globalization;

namespace LevelUp.Web.ViewModels.Character;

public sealed record CharacterSummaryViewModel
{
    public string Name { get; init; } = string.Empty;
    public int Level { get; init; }
    public decimal CurrentExperience { get; init; }
    public decimal ExperienceToNextLevel { get; init; }
    public decimal Gold { get; init; }

    public decimal ExperiencePercentage
    {
        get
        {
            if (ExperienceToNextLevel <= 0)
            {
                return 0;
            }

            decimal percentage = CurrentExperience / ExperienceToNextLevel * 100;
            return Math.Clamp(percentage, 0, 100);
        }
    }

    public string ExperienceLabel =>
        $"{CurrentExperience:N0} / {ExperienceToNextLevel:N0} XP";

    public string GoldLabel =>
        Gold.ToString("N0", CultureInfo.InvariantCulture);
}
