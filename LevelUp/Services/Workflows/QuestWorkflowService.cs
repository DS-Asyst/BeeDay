using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Rewards;
using CharacterModel = LevelUp.Domain.Character.Character;
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
    private readonly CharacterModel character;

    public QuestWorkflowService(
        QuestService questService,
        ProjectService projectService,
        MilestoneService milestoneService,
        BossService bossService,
        GameStateService gameStateService
    ) : this(
        questService,
        projectService,
        milestoneService,
        bossService,
        gameStateService,
        new CharacterModel()
    )
    {
    }

    public QuestWorkflowService(
        QuestService questService,
        ProjectService projectService,
        MilestoneService milestoneService,
        BossService bossService,
        GameStateService gameStateService,
        CharacterModel character
    )
    {
        this.questService = questService;
        this.projectService = projectService;
        this.milestoneService = milestoneService;
        this.bossService = bossService;
        this.gameStateService = gameStateService;
        this.character = character;
    }

    public QuestCompletionResult CompleteQuest(int questId)
    {
        Quest quest = questService.GetQuestById(questId)
            ?? throw new InvalidOperationException("The selected task was not found.");
        questService.CompleteQuest(quest);

        Project? project = quest.ProjectId is null
            ? null
            : projectService.GetProjectById(quest.ProjectId.Value);
        Milestone? milestone = quest.MilestoneId is null
            ? null
            : milestoneService.GetById(quest.MilestoneId.Value);

        Reward reward = new(Experience: 1m, Attribute: quest.AttributeType, AttributeExperience: 1m);
        character.ApplyReward(reward);

        bool milestoneCompleted = milestone is not null &&
            milestoneService.TryComplete(milestone, questService.GetAllQuests());
        if (milestoneCompleted && milestone is not null)
        {
            decimal chapterBonus = questService.GetQuestsByMilestoneId(milestone.Id).Count;
            Reward chapterReward = new(
                Experience: chapterBonus,
                Attribute: quest.AttributeType,
                AttributeExperience: chapterBonus
            );
            character.ApplyReward(chapterReward);
            reward = reward.Add(chapterReward);
        }
        Milestone? activatedMilestone = milestoneCompleted && milestone is not null
            ? milestoneService.UnlockAndActivateNext(milestone)
            : null;

        Quest? activatedQuest = milestoneCompleted
            ? activatedMilestone is null
                ? null
                : questService.ActivateFirstQuestForMilestone(activatedMilestone.Id)
            : questService.ActivateNextQuest(quest);

        BossEncounter? unlockedBoss = null;
        if (project is not null && bossService.TryUnlockForProject(
            project,
            questService.GetAllQuests(),
            milestoneService.GetByProjectId(project.Id)
        ))
        {
            unlockedBoss = bossService.GetByProjectId(project.Id);
        }

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
            activatedQuest,
            unlockedBoss,
            milestoneCompleted,
            false,
            milestoneProgress,
            projectProgress,
            reward
        );
    }
}
