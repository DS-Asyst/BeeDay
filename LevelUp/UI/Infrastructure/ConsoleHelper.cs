using Spectre.Console;
using LevelUp.UI.Infrastructure.Themes;

namespace LevelUp.UI;

public static class ConsoleHelper
{
    public static void ShowHeader(string title)
    {
        AnsiConsole.Clear();

        Rule rule = new($"[bold {LevelUpTheme.Primary}]" +
            $"{Markup.Escape(title.ToUpperInvariant())}[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse(LevelUpTheme.Primary)
        };

        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void ShowSuccess(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold {LevelUpTheme.Success}]" +
            $"{UIIcons.Success} Success:[/] " +
            Markup.Escape(message)
        );
    }

    public static void ShowError(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold {LevelUpTheme.Danger}]" +
            $"{UIIcons.Error} Error:[/] " +
            Markup.Escape(message)
        );
    }

    public static void ShowInformation(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[{LevelUpTheme.Information}]" +
            $"{UIIcons.Information}[/] " +
            Markup.Escape(message)
        );
    }

    public static void ShowWarning(string message)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine(
            $"[bold {LevelUpTheme.Warning}]" +
            $"{UIIcons.Warning} Aviso:[/] " +
            Markup.Escape(message)
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
                $"[bold {LevelUpTheme.Primary}]" +
                $"{Markup.Escape(title)}[/]"
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
            $"[{LevelUpTheme.MutedText}]" +
            "Pressione qualquer tecla para continuar...[/]"
        );

        Console.ReadKey(intercept: true);
    }
}
