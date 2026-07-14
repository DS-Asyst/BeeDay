using Spectre.Console;

namespace LevelUp.UI;

public static class ConsoleHelper
{
    public static void ShowHeader(string title)
    {
        AnsiConsole.Clear();

        Rule rule = new($"[bold yellow]{Markup.Escape(title.ToUpperInvariant())}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("yellow")
        };

        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void ShowSuccess(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold green]✓ Success:[/] {Markup.Escape(message)}"
        );
    }

    public static void ShowError(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold red]✗ Error:[/] {Markup.Escape(message)}"
        );
    }

    public static void ShowInformation(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[blue]ℹ[/] {Markup.Escape(message)}"
        );
    }

    public static void ShowWarning(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold yellow]⚠ Warning:[/] {Markup.Escape(message)}"
        );
    }

    public static void ShowPanel(
        string title,
        string content
    )
    {
        Panel panel = new(
            new Markup(Markup.Escape(content))
        )
        {
            Header = new PanelHeader(
                $"[bold yellow]{Markup.Escape(title)}[/]"
            ),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };

        AnsiConsole.Write(panel);
    }

    public static void ShowSeparator()
    {
        AnsiConsole.Write(
            new Rule
            {
                Style = Style.Parse("grey")
            }
        );
    }

    public static void WaitForContinue()
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            "[grey]Press any key to continue...[/]"
        );

        Console.ReadKey(intercept: true);
    }
}