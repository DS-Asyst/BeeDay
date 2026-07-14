namespace LevelUp.UI;

public static class ConsoleHelper
{
    public static void ShowHeader(string title)
    {
        Console.Clear();

        Console.WriteLine("================================");
        Console.WriteLine($"          {title.ToUpperInvariant()}");
        Console.WriteLine("================================");
        Console.WriteLine();
    }

    public static void ShowSuccess(string message)
    {
        Console.WriteLine();
        Console.WriteLine($"Success: {message}");
    }

    public static void ShowError(string message)
    {
        Console.WriteLine();
        Console.WriteLine($"Error: {message}");
    }

    public static void ShowInformation(string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
    }

    public static void ShowSeparator()
    {
        Console.WriteLine("--------------------------------");
    }
}