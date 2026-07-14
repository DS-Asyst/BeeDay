using LevelUp.Domain.Habits;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Training;

public sealed class TrainingTable
{
    private readonly IReadOnlyCollection<Habit> _habits;

    public TrainingTable(IEnumerable<Habit> habits)
    {
        ArgumentNullException.ThrowIfNull(habits);

        _habits = habits.ToList();
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Primary}]Trainings[/]"
            )
        };

        table.AddColumn(
            new TableColumn("[bold]ID[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Title[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Attribute[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Duration[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]XP[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Attribute XP[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Completions[/]").Centered()
        );

        foreach (Habit habit in _habits)
        {
            table.AddRow(
                habit.Id.ToString(),
                Markup.Escape(habit.Title),
                habit.AttributeType.ToString(),
                $"{habit.DurationInMinutes} min",
                habit.ExperienceReward.ToString("0.##"),
                habit.AttributeExperienceReward.ToString("0.##"),
                habit.TimesCompleted.ToString()
            );
        }

        table.Expand();

        return table;
    }
}