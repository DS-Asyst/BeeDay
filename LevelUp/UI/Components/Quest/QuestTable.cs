using LevelUp.Domain.Quests;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using QuestModel = LevelUp.Domain.Quests.Quest;


namespace LevelUp.UI.Components.Quest;

public sealed class QuestTable
{
    private readonly IReadOnlyCollection<QuestModel> quests;
    private readonly Func<int?, string> projectNameResolver;

    public QuestTable(
        IEnumerable<QuestModel> quests,
        Func<int?, string> projectNameResolver
    )
    {
        ArgumentNullException.ThrowIfNull(quests);
        ArgumentNullException.ThrowIfNull(
            projectNameResolver
        );

        this.quests = quests.ToList();
        this.projectNameResolver =
            projectNameResolver;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Quest}]" +
                $"{UIIcons.Quest} Painel de Missões[/]"
            )
        };

        table.AddColumn(
            new TableColumn("[bold]ID[/]")
                .Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Missão[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Status[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Projeto[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Criada em[/]")
                .Centered()
        );

        table.AddColumn(
            new TableColumn("[bold]Concluída em[/]")
                .Centered()
        );

        foreach (QuestModel quest in quests)
        {
            string projectName =
                projectNameResolver(
                    quest.ProjectId
                );

            table.AddRow(
                quest.Id.ToString(),
                Markup.Escape(quest.Title),
                QuestStatusFormatter.Format(
                    quest.Status
                ),
                Markup.Escape(projectName),
                quest.CreatedAt.ToString(
                    "dd/MM/yyyy"
                ),
                quest.CompletedAt?.ToString(
                    "dd/MM/yyyy"
                ) ?? "—"
            );
        }

        table.Expand();

        return table;
    }
}
