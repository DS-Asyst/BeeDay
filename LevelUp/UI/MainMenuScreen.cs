namespace LevelUp.UI;

public class MainMenuScreen
{
    private readonly InputReader inputReader;

    public MainMenuScreen(
        InputReader inputReader)
    {
        this.inputReader = inputReader;
    }

    public int Show()
    {
        Console.Clear();

        Console.WriteLine("====== LEVEL UP ======");
        Console.WriteLine();
        Console.WriteLine("1 - Character");
        Console.WriteLine("2 - Training");
        Console.WriteLine("3 - Projects");
        Console.WriteLine("4 - Quests");
        Console.WriteLine("5 - Gold");
        Console.WriteLine("0 - Exit");
        Console.WriteLine();

        return inputReader.ReadOption(
            "Option: ",
            0,
            5);
    }
}