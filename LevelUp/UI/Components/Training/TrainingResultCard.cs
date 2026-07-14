using LevelUp.Models;
using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Infrastructure.Themes;
using LevelUp.UI.Layout;
using Spectre.Console;
using CharacterModel = LevelUp.Models.Character;

namespace LevelUp.UI.Components.Training;

public sealed class TrainingResultCard
{
    private readonly Habit _habit;
    private readonly CharacterModel _character;
    private readonly decimal _experienceEarned;

    public TrainingResultCard(
        Habit habit,
        CharacterModel character,
        decimal experienceEarned
    )
    {
        ArgumentNullException.ThrowIfNull(habit);
        ArgumentNullException.ThrowIfNull(character);

        _habit = habit;
        _character = character;
        _experienceEarned = experienceEarned;
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
                "Training",
                _habit.Title,
                $"bold {LevelUpTheme.Text}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experience earned",
                $"+{_experienceEarned:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Attribute",
                _habit.AttributeType.ToString(),
                LevelUpTheme.Accent
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Attribute experience",
                $"+{_habit.AttributeExperienceReward:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Completions",
                _habit.TimesCompleted.ToString()
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Character level",
                _character.Level.ToString(),
                $"bold {LevelUpTheme.Primary}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Character experience",
                $"{_character.Experience:0.##}/" +
                $"{_character.ExperienceToNextLevel:0.##}"
            )
        );

        return PanelBuilder.Build(
            title: "Training Completed",
            content: summary,
            icon: UIIcons.Success,
            expand: false
        );
    }
}