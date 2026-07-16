using LevelUp.Domain.Milestones;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Milestone;

public sealed class MilestoneTable
{
    private readonly IReadOnlyCollection<Milestone> milestones;
    private readonly Func<Milestone, decimal> progressResolver;

    public MilestoneTable(
        IEnumerable<Milestone> milestones,
        Func<Milestone, decimal> progressResolver
    )
    {
        this.milestones = milestones.ToList();
        this.progressResolver = progressResolver;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle($"[bold {LevelUpTheme.Quest}]{UIIcons.Milestone} Milestones[/]")
        };

        table.AddColumn("Order");
        table.AddColumn("Milestone");
        table.AddColumn("Status");
        table.AddColumn("Progress");

        foreach (Milestone milestone in milestones.OrderBy(item => item.Order))
        {
            table.AddRow(
                milestone.Order.ToString(),
                Markup.Escape(
                    milestone.IsLocked ? "Locked milestone" : milestone.Title
                ),
                MilestoneStatusFormatter.Format(milestone.Status),
                $"{progressResolver(milestone):0.##}%"
            );
        }

        table.Expand();
        return table;
    }
}
