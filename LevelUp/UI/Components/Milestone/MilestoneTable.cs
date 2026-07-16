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
                $"{UIIcons.Milestone} Capítulos[/]"
            )
        };

        table.AddColumn("Ordem");
        table.AddColumn("Capítulo");
        table.AddColumn("Status");
        table.AddColumn("Progresso");

        foreach (MilestoneModel milestone in milestones.OrderBy(item => item.Order))
        {
            table.AddRow(
                milestone.Order.ToString(),
                Markup.Escape(
                    milestone.IsLocked
                        ? "Capítulo bloqueado"
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
