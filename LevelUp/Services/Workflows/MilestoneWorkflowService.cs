using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class MilestoneWorkflowService
{
    private readonly MilestoneService milestoneService;
    private readonly QuestService questService;
    private readonly BossService bossService;
    private readonly ProjectService projectService;
    private readonly GameStateService gameStateService;

    public MilestoneWorkflowService(
        MilestoneService milestoneService,
        QuestService questService,
        BossService bossService,
        ProjectService projectService,
        GameStateService gameStateService
    )
    {
        this.milestoneService = milestoneService;
        this.questService = questService;
        this.bossService = bossService;
        this.projectService = projectService;
        this.gameStateService = gameStateService;
    }

    public void CompleteManualMilestone(int milestoneId)
    {
        Milestone milestone = milestoneService.GetById(milestoneId)
            ?? throw new InvalidOperationException("O capítulo não foi encontrado.");
        milestoneService.CompleteManually(milestone, questService.GetAllQuests());
        milestoneService.UnlockAndActivateNext(milestone);

        var project = projectService.GetProjectById(milestone.ProjectId);
        if (project is not null)
        {
            bossService.TryUnlockForProject(
                project,
                questService.GetAllQuests(),
                milestoneService.GetByProjectId(project.Id)
            );
        }

        gameStateService.Save();
    }

    public bool DeleteMilestone(int milestoneId)
    {
        Milestone? milestone = milestoneService.GetById(milestoneId);
        if (milestone is null)
        {
            return false;
        }

        var linkedQuests = questService.GetQuestsByMilestoneId(milestoneId);
        if (linkedQuests.Any(quest => quest.Status is QuestStatus.Completed or QuestStatus.Archived))
        {
            throw new InvalidOperationException(
                "Capítulos com missões concluídas ou arquivadas não podem ser excluídos. Arquive o capítulo."
            );
        }

        foreach (var quest in linkedQuests)
        {
            questService.RemoveQuestFromMilestone(quest);
        }

        bool deleted = milestoneService.Delete(milestoneId);
        if (deleted)
        {
            gameStateService.Save();
        }
        return deleted;
    }
}
