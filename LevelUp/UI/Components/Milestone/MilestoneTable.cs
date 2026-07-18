using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using MilestoneModel = LevelUp.Domain.Milestones.Milestone;

namespace LevelUp.UI.Components.Milestone;

public sealed class MilestoneTable
{
    private readonly IReadOnlyCollection<MilestoneModel> milestones;
    private readonly Func<MilestoneModel, decimal> progressResolver;

    public MilestoneTable(
        IEnumerable<MilestoneModel> milestones,
        Func<MilestoneModel, decimal> progressResolver
    )
    {
        ArgumentNullException.ThrowIfNull(milestones);
        ArgumentNullException.ThrowIfNull(progressResolver);

        this.milestones = milestones.ToList();
        this.progressResolver = progressResolver;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Quest}]" +
                $"{UIIcons.Milestone} Milestones[/]"
            )
        };

        table.AddColumn("Ordem");
        table.AddColumn("Milestone");
        table.AddColumn("Status");
        table.AddColumn("Progress");

        foreach (MilestoneModel milestone in milestones.OrderBy(item => item.Order))
        {
            table.AddRow(
                milestone.Order.ToString(),
                Markup.Escape(
                    milestone.IsLocked
                        ? "Locked milestone"
                        : milestone.Title
                ),
                MilestoneStatusFormatter.Format(milestone.Status),
                $"{progressResolver(milestone):0.##}%"
            );
        }

        table.Expand();
        return table;
    }
}
