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
        ConsoleHelper.ShowHeader("Gold");

        ConsoleHelper.ShowInformation(
            "Esta funcionalidade ainda está em desenvolvimento."
        );

        inputReader.WaitForContinue();
    }
}