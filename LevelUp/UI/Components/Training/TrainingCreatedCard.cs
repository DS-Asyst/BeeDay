using LevelUp.Domain.Habits;
using LevelUp.UI.Infrastructure;
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
                "Título",
                _habit.Title,
                $"bold {LevelUpTheme.Text}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Descrição",
                _habit.Description
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Atributo",
                DisplayText.For(_habit.AttributeType),
                LevelUpTheme.Accent
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Recompensa de experiência",
                $"{_habit.ExperienceReward:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experiência do atributo",
                $"{_habit.AttributeExperienceReward:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        return PanelBuilder.Build(
            title: "Treinamento criado",
            content: summary,
            icon: UIIcons.Success,
            expand: false
        );
    }
}
