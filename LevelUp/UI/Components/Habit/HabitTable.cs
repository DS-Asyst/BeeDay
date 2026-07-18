using DomainHabit = LevelUp.Domain.Habits.Habit;
using LevelUp.UI.Infrastructure;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Habit;

public sealed class HabitTable
{
    private readonly IReadOnlyCollection<DomainHabit> _habits;

    public HabitTable(IEnumerable<DomainHabit> habits)
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
                $"[bold {LevelUpTheme.Primary}]Habits[/]"
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
            new TableColumn("[bold]XP[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Attribute XP[/]").Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Positive scores[/]").Centered()
        );

        foreach (DomainHabit habit in _habits)
        {
            table.AddRow(
                habit.Id.ToString(),
                Markup.Escape(habit.Title),
                DisplayText.For(habit.AttributeType),
                habit.ExperienceReward.ToString("0.##"),
                habit.AttributeExperienceReward.ToString("0.##"),
                habit.PositiveCount.ToString()
            );
        }

        table.Expand();

        return table;
    }
}
