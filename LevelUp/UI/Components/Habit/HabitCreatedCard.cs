using DomainHabit = LevelUp.Domain.Habits.Habit;
using LevelUp.UI.Infrastructure;
using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Infrastructure.Themes;
using LevelUp.UI.Layout;
using Spectre.Console;

namespace LevelUp.UI.Components.Habit;

public sealed class HabitCreatedCard
{
    private readonly DomainHabit _habit;

    public HabitCreatedCard(DomainHabit habit)
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
                DisplayText.For(_habit.AttributeType),
                LevelUpTheme.Accent
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
            title: "Habit created",
            content: summary,
            icon: UIIcons.Success,
            expand: false
        );
    }
}
