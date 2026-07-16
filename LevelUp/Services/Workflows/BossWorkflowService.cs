using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class BossWorkflowService
{
    private readonly BossService bossService;
    private readonly MilestoneService milestoneService;
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly GameStateService gameStateService;

    public BossWorkflowService(
        BossService bossService,
        MilestoneService milestoneService,
        ProjectService projectService,
        QuestService questService,
        GameStateService gameStateService
    )
    {
        this.bossService = bossService;
        this.milestoneService = milestoneService;
        this.projectService = projectService;
        this.questService = questService;
        this.gameStateService = gameStateService;
    }

    public BossDefeatResult Defeat(int milestoneId)
    {
        Milestone milestone = milestoneService.GetById(milestoneId)
            ?? throw new InvalidOperationException("O capítulo não foi encontrado.");
        BossEncounter boss = bossService.GetByMilestoneId(milestoneId)
            ?? throw new InvalidOperationException("O capítulo não possui encontro com chefe.");
        Project project = projectService.GetProjectById(milestone.ProjectId)
            ?? throw new InvalidOperationException("O projeto não foi encontrado.");

        bossService.Defeat(boss);
        milestone.Complete();
        Milestone? next = milestoneService.UnlockAndActivateNext(milestone);

        bool projectCompleted = projectService.TryCompleteProject(
                project,
                questService.GetAllQuests(),
                milestoneService.GetByProjectId(project.Id)
            );

        gameStateService.Save();
        return new BossDefeatResult(boss, milestone, next, project, projectCompleted);
    }
}
