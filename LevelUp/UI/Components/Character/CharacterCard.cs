using LevelUp.Models;
using LevelUp.UI.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Cards;

public sealed class CharacterCard
{
    private const int ExperienceBarWidth = 30;

    private readonly Character _character;

    public CharacterCard(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);

        _character = character;
    }

    public Panel Build()
    {
        Grid summary = new();

        summary.AddColumn(
            new GridColumn().NoWrap()
        );

        summary.AddColumn();

        summary.AddRow(
            $"[bold {LevelUpTheme.MutedText}]Name[/]",
            $"[bold {LevelUpTheme.Text}]" +
            $"{Markup.Escape(_character.Name)}[/]"
        );

        summary.AddRow(
            $"[bold {LevelUpTheme.MutedText}]Level[/]",
            $"[bold {LevelUpTheme.Primary}]" +
            $"{_character.Level}[/]"
        );

        summary.AddRow(
            $"[bold {LevelUpTheme.MutedText}]Experience[/]",
            $"{_character.Experience}/" +
            $"{_character.ExperienceToNextLevel}"
        );

        summary.AddRow(
            new Markup(
                $"[bold {LevelUpTheme.MutedText}]Progress[/]"
            ),
            BuildExperienceBar()
        );

        return new Panel(summary)
        {
            Header = new PanelHeader(
                $"[bold {LevelUpTheme.Primary}]" +
                $"{UIIcons.Character} Character Summary[/]"
            ),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };
    }

    private Markup BuildExperienceBar()
    {
        decimal percentage = CalculateExperiencePercentage();

        int completedBlocks = (int)Math.Round(
            percentage / 100 * ExperienceBarWidth
        );

        completedBlocks = Math.Clamp(
            completedBlocks,
            0,
            ExperienceBarWidth
        );

        int remainingBlocks =
            ExperienceBarWidth - completedBlocks;

        string completed = new('█', completedBlocks);
        string remaining = new('░', remainingBlocks);

        string content =
            $"[{LevelUpTheme.Experience}]{completed}[/]" +
            $"[{LevelUpTheme.MutedText}]{remaining}[/] " +
            $"[{LevelUpTheme.Primary}]{percentage:0}%[/]";

        return new Markup(content);
    }

    private decimal CalculateExperiencePercentage()
    {
        if (_character.ExperienceToNextLevel <= 0)
        {
            return 0;
        }

        decimal percentage =
            _character.Experience /
            _character.ExperienceToNextLevel *
            100;

        return Math.Clamp(percentage, 0, 100);
    }
}