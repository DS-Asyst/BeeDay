using LevelUp.Domain.Quests;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using ProjectModel = LevelUp.Domain.Projects.Project;

namespace LevelUp.UI.Components.Project;

public sealed class ProjectTable
{
    private readonly IReadOnlyCollection<ProjectModel> projects;
    private readonly Func<int, IReadOnlyList<Quest>> questResolver;
    private readonly Func<ProjectModel, decimal> progressResolver;

    public ProjectTable(
        IEnumerable<ProjectModel> projects,
        Func<int, IReadOnlyList<Quest>> questResolver,
        Func<ProjectModel, decimal> progressResolver
    )
    {
        ArgumentNullException.ThrowIfNull(projects);
        ArgumentNullException.ThrowIfNull(questResolver);
        ArgumentNullException.ThrowIfNull(progressResolver);

        this.projects = projects.ToList();
        this.questResolver = questResolver;
        this.progressResolver = progressResolver;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Boss}]" +
                $"{UIIcons.Project} Project Board[/]"
            )
        };

        table.AddColumn(new TableColumn("[bold]ID[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Project[/]"));
        table.AddColumn(new TableColumn("[bold]Status[/]"));
        table.AddColumn(new TableColumn("[bold]Quests[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Progress[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Unlocked Title[/]"));

        foreach (ProjectModel project in projects)
        {
            IReadOnlyList<Quest> quests = questResolver(project.Id)
                .Where(quest => quest.Status != QuestStatus.Archived)
                .ToList();
            int completed = quests.Count(
                quest => quest.Status == QuestStatus.Completed
            );

            table.AddRow(
                project.Id.ToString(),
                Markup.Escape(project.Name),
                ProjectStatusFormatter.Format(project.Status),
                $"{completed}/{quests.Count}",
                $"{progressResolver(project):0.##}%",
                Markup.Escape(project.UnlockedTitle)
            );
        }

        table.Expand();
        return table;
    }
}
