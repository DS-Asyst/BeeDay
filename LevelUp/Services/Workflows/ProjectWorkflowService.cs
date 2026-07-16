using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class ProjectWorkflowService
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly BossService bossService;
    private readonly GameStateService gameStateService;

    public ProjectWorkflowService(
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        BossService bossService,
        GameStateService gameStateService
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.bossService = bossService;
        this.gameStateService = gameStateService;
    }

    public bool DeleteProject(int projectId)
    {
        Project? project = projectService.GetProjectById(projectId);
        if (project is null)
        {
            return false;
        }

        var linkedQuests = questService.GetQuestsByProjectId(projectId);
        if (linkedQuests.Any(quest => quest.Status is QuestStatus.Completed or QuestStatus.Archived))
        {
            throw new InvalidOperationException(
                "Projetos com missões concluídas ou arquivadas não podem ser excluídos. Arquive o projeto."
            );
        }

        foreach (var quest in linkedQuests)
        {
            if (quest.MilestoneId is not null)
            {
                questService.RemoveQuestFromMilestone(quest);
            }

            questService.RemoveQuestFromProject(quest);
        }

        foreach (var milestone in milestoneService.GetByProjectId(projectId).ToList())
        {
            if (milestone.Status == MilestoneStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Projetos com capítulos concluídos não podem ser excluídos. Arquive o projeto."
                );
            }

            milestoneService.Delete(milestone.Id);
        }

        bossService.DeleteByProjectId(projectId);

        bool deleted = projectService.DeleteProject(projectId);
        if (deleted)
        {
            gameStateService.Save();
        }

        return deleted;
    }
}
