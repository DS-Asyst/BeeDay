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
                new[] { "Ficha do personagem", "Progressão", "Conquistas", "Voltar" },
                choice => choice
            );

            switch (option)
            {
                case "Ficha do personagem":
                    ShowProfile(character);
                    inputReader.WaitForContinue();
                    break;
                case "Progressão":
                    ShowProgression(character);
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

    private static void ShowProgression(CharacterModel character)
    {
        ConsoleHelper.ShowHeader("Progressão do personagem");
        Table table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("Faixa de nível");
        table.AddColumn("Título");
        table.AddColumn("Situação");
        var ranks = new (string Range, string Name, int Minimum)[]
        {
            ("1–9", "Aprendiz", 1),
            ("10–19", "Aventureiro", 10),
            ("20–29", "Discípulo", 20),
            ("30–39", "Adepto", 30),
            ("40–49", "Especialista", 40),
            ("50–59", "Mestre", 50),
            ("60+", "Lenda", 60)
        };
        foreach (var rank in ranks)
        {
            string situation = character.Level >= rank.Minimum ? "Desbloqueado" : "Bloqueado";
            table.AddRow(rank.Range, rank.Name, situation);
        }
        AnsiConsole.Write(table);
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
