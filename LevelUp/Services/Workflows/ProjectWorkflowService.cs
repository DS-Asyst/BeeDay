using LevelUp.Domain.Milestones;
using LevelUp.Domain.Attributes;
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

    public Project CreateProject(
        string name,
        string description,
        string bossName,
        string bossDescription,
        string achievementPrefix
    ) => CreateProject(name, description, bossName, bossDescription, achievementPrefix, AttributeType.Intelligence);

    public Project CreateProject(
        string name,
        string description,
        string bossName,
        string bossDescription,
        string achievementPrefix,
        AttributeType primaryAttribute
    )
    {
        Project project = projectService.CreateProject(name, description, primaryAttribute);
        bossService.CreateFinalBoss(
            project,
            bossName,
            bossDescription,
            achievementPrefix
        );
        gameStateService.Save();
        return project;
    }


    public void ActivateProject(int projectId)
    {
        Project project = projectService.GetProjectById(projectId)
            ?? throw new InvalidOperationException("O projeto não foi encontrado.");

        projectService.ActivateProject(project);
        bool milestoneActivated = milestoneService.TryActivateFirst(project);

        if (milestoneActivated)
        {
            Milestone? active = milestoneService.GetByProjectId(project.Id)
                .FirstOrDefault(item => item.Status == MilestoneStatus.Active);

            if (active is not null)
            {
                questService.ActivateFirstQuestForMilestone(active.Id);
            }
        }
        else
        {
            questService.ActivateFirstProjectQuest(project.Id);
        }

        gameStateService.Save();
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
