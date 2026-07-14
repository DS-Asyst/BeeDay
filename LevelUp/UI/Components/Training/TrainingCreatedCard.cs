using LevelUp.Models;
using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Infrastructure.Themes;
using LevelUp.UI.Layout;
using Spectre.Console;

namespace LevelUp.UI.Components.Training;

public sealed class TrainingCreatedCard
{
    private readonly Habit _habit;

    public TrainingCreatedCard(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);

        _habit = habit;
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
                "ID",
                _habit.Id.ToString()
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Title",
                _habit.Title,
                $"bold {LevelUpTheme.Text}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Description",
                _habit.Description
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
                "Duration",
                $"{_habit.DurationInMinutes} min"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experience reward",
                $"{_habit.ExperienceReward:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Attribute experience",
                $"{_habit.AttributeExperienceReward:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        return PanelBuilder.Build(
            title: "Training Created",
            content: summary,
            icon: UIIcons.Success,
            expand: false
        );
    }
}