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
            ConsoleHelper.ShowHeader("Personagem");
            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[] { "Ficha do personagem", "Conquistas", "Voltar" },
                choice => choice
            );

            switch (option)
            {
                case "Ficha do personagem":
                    ShowProfile(character);
                    inputReader.WaitForContinue();
                    break;
                case "Conquistas":
                    ShowAchievements();
                    inputReader.WaitForContinue();
                    break;
                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private static void ShowProfile(CharacterModel character)
    {
        ConsoleHelper.ShowHeader("Ficha do personagem");
        AnsiConsole.Write(new CharacterCard(character).Build());
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new AttributeTable(character.Attributes).Build());
    }

    private void ShowAchievements()
    {
        ConsoleHelper.ShowHeader("Conquistas");
        var achievements = achievementService.GetUnlocked();
        if (achievements.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhuma conquista foi desbloqueada ainda.");
            return;
        }

        AnsiConsole.Write(new AchievementTable(achievements).Build());
    }
}
