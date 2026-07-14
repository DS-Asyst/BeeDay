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
        ConsoleHelper.ShowHeader("Bosses");

        ConsoleHelper.ShowInformation(
            "Esta funcionalidade ainda está em desenvolvimento."
        );

        inputReader.WaitForContinue();
    }
}