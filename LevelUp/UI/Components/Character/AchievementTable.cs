using LevelUp.Domain.Achievements;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI.Components.Character;

public sealed class AchievementTable
{
    private readonly IReadOnlyCollection<Achievement> achievements;

    public AchievementTable(IEnumerable<Achievement> achievements)
    {
        ArgumentNullException.ThrowIfNull(achievements);
        this.achievements = achievements.ToList();
    }

    public Table Build()
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .Expand();
        table.Title = new TableTitle("[bold]Achievements[/]");
        table.AddColumn("Achievement");
        table.AddColumn("Description");
        table.AddColumn("Categoria");
        table.AddColumn("Unlocked em");

        foreach (Achievement achievement in achievements)
        {
            table.AddRow(
                Markup.Escape(achievement.Name),
                Markup.Escape(achievement.Description),
                DisplayText.For(achievement.Category),
                achievement.UnlockedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—"
            );
        }

        return table;
    }
}
