namespace LevelUp.Web.ViewModels;

public sealed record DashboardViewModel(
    string CharacterName,
    int Level,
    decimal Experience,
    decimal ExperienceToNextLevel,
    decimal Gold)
{
    public static DashboardViewModel Preview { get; } = new(
        CharacterName: "Tiago",
        Level: 8,
        Experience: 640,
        ExperienceToNextLevel: 1000,
        Gold: 12_540);
}
