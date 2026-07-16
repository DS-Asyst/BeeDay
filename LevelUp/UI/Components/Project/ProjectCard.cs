using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure;
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
    private readonly BossEncounter? boss;

    public ProjectCard(
        ProjectModel project,
        IEnumerable<QuestModel> quests,
        decimal progress,
        IEnumerable<MilestoneModel>? milestones = null,
        BossEncounter? boss = null
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(quests);
        this.project = project;
        this.quests = quests.ToList();
        this.progress = progress;
        this.milestones = milestones?.ToList() ?? [];
        this.boss = boss;
    }

    public Panel Build()
    {
        int completedQuests = quests.Count(quest => quest.Status == QuestStatus.Completed);
        int completedMilestones = milestones.Count(item => item.Status == MilestoneStatus.Completed);
        EntityCard card = new(project.Name, UIIcons.Project);
        card.AddText("ID", project.Id.ToString());
        card.AddMarkup("Status", ProjectStatusFormatter.Format(project.Status));
        card.AddText("Descrição", project.Description);
        card.AddText("Progresso das missões", $"{progress:0.##}%", LevelUpTheme.Success);
        card.AddText("Missões", $"{completedQuests}/{quests.Count}");
        card.AddText("Capítulos", $"{completedMilestones}/{milestones.Count}");
        card.AddText(
            "Chefe final",
            boss is null ? "Não configurado" : $"{boss.Name} — {DisplayText.For(boss.Status)}",
            LevelUpTheme.Boss
        );
        if (boss is not null)
        {
            card.AddText(
                "Conquista",
                $"{boss.AchievementPrefix} {boss.Name}",
                LevelUpTheme.Gold
            );
        }
        card.AddText("Criado em", project.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
        if (project.CompletedAt is not null)
        {
            card.AddText("Concluído em", project.CompletedAt.Value.ToString("dd/MM/yyyy HH:mm"), LevelUpTheme.Success);
        }
        if (project.ArchivedAt is not null)
        {
            card.AddText("Arquivado em", project.ArchivedAt.Value.ToString("dd/MM/yyyy HH:mm"), LevelUpTheme.MutedText);
        }
        return card.Build();
    }
}
