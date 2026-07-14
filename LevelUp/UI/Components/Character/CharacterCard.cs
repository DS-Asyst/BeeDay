using LevelUp.Models;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using CharacterModel = LevelUp.Models.Character;
using LevelUp.UI.Layout;
using LevelUp.UI.Infrastructure.Builders;

namespace LevelUp.UI.Components.Character;

public sealed class CharacterCard
{
    private const int ExperienceBarWidth = 30;

    private readonly CharacterModel _character;

    public CharacterCard(CharacterModel character)
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
            StatisticRow.Build(
                "Name",
                _character.Name,
                $"bold {LevelUpTheme.Text}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Level",
                _character.Level.ToString(),
                $"bold {LevelUpTheme.Primary}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experience",
                $"{_character.Experience}/" +
                $"{_character.ExperienceToNextLevel}"
            )
        );

        summary.AddRow(
            new Markup(
                $"[bold {LevelUpTheme.MutedText}]Progress[/]"
            ),
            BuildExperienceBar()
        );

        return PanelBuilder.Build(
            title: "Character",
            content: summary,
            icon: UIIcons.Character,
            expand: false
        );
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