using LevelUp.Services.Achievements;
using LevelUp.UI.Components.Character;
using Spectre.Console;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public sealed class CharacterScreen
{
    private readonly InputReader inputReader;
    private readonly AchievementService achievementService;

    public CharacterScreen(
        InputReader inputReader,
        AchievementService achievementService
    )
    {
        this.inputReader = inputReader;
        this.achievementService = achievementService;
    }

    public void Show(CharacterModel character)
    {
        bool running = true;
        while (running)
        {
            ConsoleHelper.ShowHeader("Character");
            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[] { "Profile", "Achievements", "Back" },
                choice => choice
            );

            switch (option)
            {
                case "Profile":
                    ShowProfile(character);
                    inputReader.WaitForContinue();
                    break;
                case "Achievements":
                    ShowAchievements();
                    inputReader.WaitForContinue();
                    break;
                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private static void ShowProfile(CharacterModel character)
    {
        ConsoleHelper.ShowHeader("Profile");
        AnsiConsole.Write(new CharacterCard(character).Build());
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new AttributeTable(character.Attributes).Build());
    }

    private void ShowAchievements()
    {
        ConsoleHelper.ShowHeader("Achievements");
        var achievements = achievementService.GetUnlocked();
        if (achievements.Count == 0)
        {
            ConsoleHelper.ShowInformation("No achievements have been unlocked yet.");
            return;
        }

        AnsiConsole.Write(new AchievementTable(achievements).Build());
    }
}
