using LevelUp.UI.Components.Shared;
using Spectre.Console;

namespace LevelUp.UI;

public class BossScreen
{
    private readonly InputReader inputReader;

    public BossScreen(InputReader inputReader)
    {
        this.inputReader = inputReader;
    }

    public void Show()
    {
        ConsoleHelper.ShowHeader("Projects");

        AnsiConsole.Write(
            new ComingSoonCard("Projects").Build()
        );

        inputReader.WaitForContinue();
    }
}