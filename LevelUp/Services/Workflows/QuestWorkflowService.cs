using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class QuestWorkflowService
{
    private readonly QuestService questService;
    private readonly ProjectService projectService;
    private readonly MilestoneService milestoneService;
    private readonly BossService bossService;
    private readonly GameStateService gameStateService;

    public QuestWorkflowService(
        QuestService questService,
        ProjectService projectService,
        MilestoneService milestoneService,
        BossService bossService,
        GameStateService gameStateService
    )
    {
        this.questService = questService;
        this.projectService = projectService;
        this.milestoneService = milestoneService;
        this.bossService = bossService;
        this.gameStateService = gameStateService;
    }

    public QuestCompletionResult CompleteQuest(int questId)
    {
        Quest quest = questService.GetQuestById(questId)
            ?? throw new InvalidOperationException("A missão selecionada não foi encontrada.");

        questService.CompleteQuest(quest);

        Project? project = quest.ProjectId is null
            ? null
            : projectService.GetProjectById(quest.ProjectId.Value);

        Milestone? milestone = quest.MilestoneId is null
            ? null
            : milestoneService.GetById(quest.MilestoneId.Value);

        BossEncounter? configuredBoss = milestone is null
            ? null
            : bossService.GetByMilestoneId(milestone.Id);

        bool requirementsMet = milestone is not null &&
            milestoneService.HasMetRequirements(milestone, questService.GetAllQuests());

        bool milestoneCompleted = milestone is not null &&
            configuredBoss is null &&
            milestoneService.TryComplete(milestone, questService.GetAllQuests());

        Milestone? activatedMilestone = milestoneCompleted && milestone is not null
            ? milestoneService.UnlockAndActivateNext(milestone)
            : null;

        BossEncounter? unlockedBoss = null;
        if (requirementsMet && configuredBoss is not null &&
            bossService.TryUnlockForMilestoneRequirement(milestone!, requirementsMet))
        {
            unlockedBoss = configuredBoss;
        }

        bool projectCompleted = project is not null &&
            projectService.TryCompleteProject(
                project,
                questService.GetAllQuests(),
                milestoneService.GetByProjectId(project.Id)
            );

        decimal milestoneProgress = milestone is null
            ? 0m
            : milestoneService.CalculateProgress(milestone, questService.GetAllQuests());

        decimal projectProgress = project is null
            ? 0m
            : projectService.CalculateProgress(project, questService.GetAllQuests());

        gameStateService.Save();

        return new QuestCompletionResult(
            quest,
            project,
            milestone,
            activatedMilestone,
            unlockedBoss,
            milestoneCompleted,
            projectCompleted,
            milestoneProgress,
            projectProgress
        );
    }
}
