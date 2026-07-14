using LevelUp.UI.Components.Shared;
using Spectre.Console;

namespace LevelUp.UI;

public class QuestScreen
{
    private readonly InputReader inputReader;

    public QuestScreen(InputReader inputReader)
    {
        this.inputReader = inputReader;
    }

    public void Show()
    {
        ConsoleHelper.ShowHeader("Quests");

        AnsiConsole.Write(
            new ComingSoonCard("Quests").Build()
        );

        inputReader.WaitForContinue();
    }
}