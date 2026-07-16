using LevelUp.UI.Components.Shared;
using Spectre.Console;

namespace LevelUp.UI;

public class GoldScreen
{
    private readonly InputReader inputReader;

    public GoldScreen(InputReader inputReader)
    {
        this.inputReader = inputReader;
    }

    public void Show()
    {
        ConsoleHelper.ShowHeader("Carteira");

        AnsiConsole.Write(
            new ComingSoonCard("Carteira").Build()
        );

        inputReader.WaitForContinue();
    }
}
