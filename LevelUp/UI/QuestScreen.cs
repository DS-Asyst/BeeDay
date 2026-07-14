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

        ConsoleHelper.ShowInformation(
            "Esta funcionalidade ainda está em desenvolvimento."
        );

        inputReader.WaitForContinue();
    }
}