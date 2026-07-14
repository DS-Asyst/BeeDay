using LevelUp.UI.Components.Shared;
using Spectre.Console;

namespace LevelUp.UI;

public class ProjectScreen
{
    private readonly InputReader inputReader;

    public ProjectScreen(InputReader inputReader)
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