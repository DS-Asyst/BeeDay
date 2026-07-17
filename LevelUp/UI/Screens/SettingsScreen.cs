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
        ConsoleHelper.ShowHeader("Configurações");

        Table table = new Table()
            .Border(TableBorder.Rounded);

        table.AddColumn("Configuração");
        table.AddColumn("Valor");
        table.AddRow("Idioma", "Português (Brasil)");
        table.AddRow(
            "Versão do schema",
            LevelUp.Domain.GameData.CurrentSchemaVersion.ToString()
        );
        table.AddRow(
            "Revisão do save",
            gameStateService.CurrentSaveRevision.ToString()
        );
        table.AddRow(
            "Último salvamento",
            gameStateService.LastSavedAt?.ToString("dd/MM/yyyy HH:mm:ss")
                ?? "Ainda não salvo"
        );

        AnsiConsole.Write(table);
        inputReader.WaitForContinue();
    }
}
