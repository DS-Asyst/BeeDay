using LevelUp.Models;
using Spectre.Console;
using LevelUp.UI.Components;
using LevelUp.UI.Themes;

namespace LevelUp.UI;

public class CharacterScreen
{
    private const int ProgressBarWidth = 30;

    public void Show(Character character)
    {
        ConsoleHelper.ShowHeader("Character");

        ShowCharacterSummary(character);

        AnsiConsole.WriteLine();

        ShowAttributes(character);
    }

    private static void ShowCharacterSummary(Character character)
    {
        string experienceBar = ExperienceBar.Render(
            character.Experience,
            character.ExperienceToNextLevel,
            ProgressBarWidth
        );

        Grid summary = new();

        summary.AddColumn();
        summary.AddColumn();

        summary.AddRow(
            "[bold grey]Name[/]",
            $"[bold white]{Markup.Escape(character.Name)}[/]"
        );

        summary.AddRow(
            "[bold grey]Level[/]",
            $"[bold yellow]{character.Level}[/]"
        );

        summary.AddRow(
            "[bold grey]Experience[/]",
            $"{character.Experience}/{character.ExperienceToNextLevel}"
        );

        summary.AddRow(
            "[bold grey]Progress[/]",
            experienceBar
        );

        Panel panel = new(summary)
        {
            Header = new PanelHeader("[bold yellow]Character Summary[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };

        AnsiConsole.Write(panel);
    }

    private static void ShowAttributes(Character character)
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle("[bold yellow]Attributes[/]")
        };

        table.AddColumn(
            new TableColumn("[bold]Attribute[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Level[/]").Centered()
        );

        table.AddRow(
            "Strength",
            FormatLevel(character.Attributes.Strength.Level)
        );

        table.AddRow(
            "Intelligence",
            FormatLevel(character.Attributes.Intelligence.Level)
        );

        table.AddRow(
            "Vitality",
            FormatLevel(character.Attributes.Vitality.Level)
        );

        table.AddRow(
            "Agility",
            FormatLevel(character.Attributes.Agility.Level)
        );

        table.AddRow(
            "Dexterity",
            FormatLevel(character.Attributes.Dexterity.Level)
        );

        table.AddRow(
            "Luck",
            FormatLevel(character.Attributes.Luck.Level)
        );

        table.Expand();

        AnsiConsole.Write(table);
    }

    private static string FormatLevel(int level)
    {
        return $"[bold cyan]{level}[/]";
    }

}