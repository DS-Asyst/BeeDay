using LevelUp.Domain.Quests;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using ProjectModel = LevelUp.Domain.Projects.Project;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI.Components.Project;

public sealed class ProjectCard
{
    private readonly ProjectModel project;
    private readonly IReadOnlyCollection<QuestModel> quests;
    private readonly decimal progress;

    public ProjectCard(
        ProjectModel project,
        IEnumerable<QuestModel> quests,
        decimal progress
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(quests);

        this.project = project;
        this.quests = quests.ToList();
        this.progress = progress;
    }

    public Panel Build()
    {
        int completed = quests.Count(
            quest => quest.Status == QuestStatus.Completed
        );

        EntityCard card = new(
            project.Name,
            UIIcons.Project
        );

        card.AddText("ID", project.Id.ToString());
        card.AddMarkup(
            "Status",
            ProjectStatusFormatter.Format(project.Status)
        );
        card.AddText("Description", project.Description);
        card.AddText(
            "Progress",
            $"{progress:0.##}%",
            LevelUpTheme.Success
        );
        card.AddText("Quests", $"{completed}/{quests.Count}");
        card.AddText(
            "Unlocked title",
            project.UnlockedTitle,
            LevelUpTheme.Gold
        );
        card.AddText(
            "Created",
            project.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        );

        if (project.CompletedAt is not null)
        {
            card.AddText(
                "Completed",
                project.CompletedAt.Value.ToString(
                    "dd/MM/yyyy HH:mm"
                ),
                LevelUpTheme.Success
            );
        }

        if (project.ArchivedAt is not null)
        {
            card.AddText(
                "Archived",
                project.ArchivedAt.Value.ToString(
                    "dd/MM/yyyy HH:mm"
                ),
                LevelUpTheme.MutedText
            );
        }

        return card.Build();
    }
}
