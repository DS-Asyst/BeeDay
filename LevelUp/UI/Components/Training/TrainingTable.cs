using LevelUp.Domain.Habits;
using LevelUp.UI.Infrastructure;
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
                $"[bold {LevelUpTheme.Primary}]Treinamentos[/]"
            )
        };

        table.AddColumn(
            new TableColumn("[bold]ID[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Título[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Atributo[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Duração[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]XP[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]XP do atributo[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Conclusões[/]").Centered()
        );

        foreach (Habit habit in _habits)
        {
            table.AddRow(
                habit.Id.ToString(),
                Markup.Escape(habit.Title),
                DisplayText.For(habit.AttributeType),
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
