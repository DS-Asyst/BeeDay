using LevelUp.Web.ViewModels.Character;

namespace LevelUp.Web.ViewModels;

public sealed record DashboardViewModel(CharacterSummaryViewModel Character)
{
    public static DashboardViewModel Preview { get; } = new(
        new CharacterSummaryViewModel
        {
            Name = "Tiago",
            Level = 8,
            CurrentExperience = 640,
            ExperienceToNextLevel = 1000,
            Gold = 12_540
        });
}
