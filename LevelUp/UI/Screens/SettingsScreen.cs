using Spectre.Console;

namespace LevelUp.UI;

public sealed class SettingsScreen
{
    private readonly InputReader inputReader;

    public SettingsScreen(InputReader inputReader)
    {
        this.inputReader = inputReader;
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
            "Versão do save",
            LevelUp.Domain.GameData.CurrentSchemaVersion.ToString()
        );

        AnsiConsole.Write(table);
        inputReader.WaitForContinue();
    }
}
