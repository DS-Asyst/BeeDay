using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using MilestoneModel = LevelUp.Domain.Milestones.Milestone;
using ProjectModel = LevelUp.Domain.Projects.Project;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI.Components.Project;

public sealed class ProjectCard
{
    private readonly ProjectModel project;
    private readonly IReadOnlyCollection<QuestModel> quests;
    private readonly decimal progress;
    private readonly IReadOnlyCollection<MilestoneModel> milestones;

    public ProjectCard(
        ProjectModel project,
        IEnumerable<QuestModel> quests,
        decimal progress,
        IEnumerable<MilestoneModel>? milestones = null
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(quests);

        this.project = project;
        this.quests = quests.ToList();
        this.progress = progress;
        this.milestones = milestones?.ToList() ?? [];
    }

    public Panel Build()
    {
        int completedQuests = quests.Count(
            quest => quest.Status == QuestStatus.Completed
        );

        int completedMilestones = milestones.Count(
            milestone => milestone.Status == MilestoneStatus.Completed
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
        card.AddText("Quests", $"{completedQuests}/{quests.Count}");
        card.AddText(
            "Milestones",
            $"{completedMilestones}/{milestones.Count}"
        );
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
                project.CompletedAt.Value.ToString("dd/MM/yyyy HH:mm"),
                LevelUpTheme.Success
            );
        }

        if (project.ArchivedAt is not null)
        {
            card.AddText(
                "Archived",
                project.ArchivedAt.Value.ToString("dd/MM/yyyy HH:mm"),
                LevelUpTheme.MutedText
            );
        }

        return card.Build();
    }
}
