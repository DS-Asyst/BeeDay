using LevelUp.Services.Persistence;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class SettingsScreen
{
    private readonly InputReader inputReader;
    private readonly GameStateService gameStateService;

    public SettingsScreen(
        InputReader inputReader,
        GameStateService gameStateService
    )
    {
        this.inputReader = inputReader;
        this.gameStateService = gameStateService;
    }

    public void Show()
    {
        ConsoleHelper.ShowHeader("Settings");

        Table table = new Table()
            .Border(TableBorder.Rounded);

        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.AddRow("Language", "English");
        table.AddRow(
            "Schema version",
            LevelUp.Domain.GameData.CurrentSchemaVersion.ToString()
        );
        table.AddRow(
            "Save revision",
            gameStateService.CurrentSaveRevision.ToString()
        );
        table.AddRow(
            "Last saved",
            gameStateService.LastSavedAt?.ToString("dd/MM/yyyy HH:mm:ss")
                ?? "Not saved yet"
        );

        AnsiConsole.Write(table);
        inputReader.WaitForContinue();
    }
}
