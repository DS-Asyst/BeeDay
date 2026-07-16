using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using MilestoneModel = LevelUp.Domain.Milestones.Milestone;
using ProjectModel = LevelUp.Domain.Projects.Project;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI.Components.Project;

public sealed class ProjectTable
{
    private readonly IReadOnlyCollection<ProjectModel> projects;
    private readonly Func<int, IReadOnlyList<QuestModel>> questResolver;
    private readonly Func<ProjectModel, decimal> progressResolver;
    private readonly Func<int, IReadOnlyList<MilestoneModel>> milestoneResolver;

    public ProjectTable(
        IEnumerable<ProjectModel> projects,
        Func<int, IReadOnlyList<QuestModel>> questResolver,
        Func<ProjectModel, decimal> progressResolver,
        Func<int, IReadOnlyList<MilestoneModel>> milestoneResolver
    )
    {
        ArgumentNullException.ThrowIfNull(projects);
        ArgumentNullException.ThrowIfNull(questResolver);
        ArgumentNullException.ThrowIfNull(progressResolver);
        ArgumentNullException.ThrowIfNull(milestoneResolver);

        this.projects = projects.ToList();
        this.questResolver = questResolver;
        this.progressResolver = progressResolver;
        this.milestoneResolver = milestoneResolver;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Boss}]" +
                $"{UIIcons.Project} Painel de Projetos[/]"
            )
        };

        table.AddColumn(new TableColumn("[bold]ID[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Projeto[/]"));
        table.AddColumn(new TableColumn("[bold]Status[/]"));
        table.AddColumn(new TableColumn("[bold]Missões[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Progresso[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Capítulos[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Título desbloqueado[/]"));

        foreach (ProjectModel project in projects)
        {
            IReadOnlyList<QuestModel> quests = questResolver(project.Id)
                .Where(quest => quest.Status != QuestStatus.Archived)
                .ToList();

            int completedQuests = quests.Count(
                quest => quest.Status == QuestStatus.Completed
            );

            IReadOnlyList<MilestoneModel> milestones = milestoneResolver(project.Id)
                .Where(milestone => milestone.Status != MilestoneStatus.Archived)
                .ToList();

            int completedMilestones = milestones.Count(
                milestone => milestone.Status == MilestoneStatus.Completed
            );

            table.AddRow(
                project.Id.ToString(),
                Markup.Escape(project.Name),
                ProjectStatusFormatter.Format(project.Status),
                $"{completedQuests}/{quests.Count}",
                $"{progressResolver(project):0.##}%",
                $"{completedMilestones}/{milestones.Count}",
                Markup.Escape(project.UnlockedTitle)
            );
        }

        table.Expand();
        return table;
    }
}
