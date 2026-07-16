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
        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Configuração");
        table.AddColumn("Valor");
        table.AddRow("Idioma da interface", "Português (Brasil)");
        table.AddRow("Idioma do código", "Inglês");
        table.AddRow("Salvamento", "Automático e manual");
        table.AddRow("Comando de cancelamento", "cancel");
        AnsiConsole.Write(table);
        inputReader.WaitForContinue();
    }
}
