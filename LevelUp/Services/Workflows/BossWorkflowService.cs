using LevelUp.Domain.Achievements;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Projects;
using LevelUp.Services.Achievements;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class BossWorkflowService
{
    private readonly BossService bossService;
    private readonly AchievementService achievementService;
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly GameStateService gameStateService;

    public BossWorkflowService(
        BossService bossService,
        AchievementService achievementService,
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        GameStateService gameStateService
    )
    {
        this.bossService = bossService;
        this.achievementService = achievementService;
        this.projectService = projectService;
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.gameStateService = gameStateService;
    }

    public BossDefeatResult Defeat(int projectId)
    {
        Project project = projectService.GetProjectById(projectId)
            ?? throw new InvalidOperationException("The project was not found.");
        BossEncounter boss = bossService.GetByProjectId(projectId)
            ?? throw new InvalidOperationException("The project does not have a final boss.");

        if (!projectService.AreCompletionRequirementsMet(
            project,
            questService.GetAllQuests(),
            milestoneService.GetByProjectId(project.Id)
        ))
        {
            throw new InvalidOperationException(
                "Complete all tasks and milestones before facing the final boss."
            );
        }

        if (boss.Status == BossStatus.Locked)
        {
            boss.Unlock();
        }
        bossService.Defeat(boss);
        projectService.CompleteProject(project);
        Achievement achievement = achievementService.UnlockProjectAchievement(project, boss);
        gameStateService.Save();
        return new BossDefeatResult(boss, project, achievement, true);
    }
}
