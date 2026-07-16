using LevelUp.Domain;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using LevelUp.Services.Workflows;
using Xunit;

namespace LevelUp.Tests;

public sealed class MilestoneWorkflowTests
{
    [Fact]
    public void CompletingLastQuestCompletesMilestoneAndActivatesNext()
    {
        TestContext context = new();

        var project = context.Projects.CreateProject(
            "Project",
            "Description",
            "Reward"
        );

        context.Projects.ActivateProject(project);

        var first = context.Milestones.CreateMilestone(
            project,
            "First",
            "Description",
            1
        );

        var second = context.Milestones.CreateMilestone(
            project,
            "Second",
            "Description",
            2
        );

        context.Milestones.Activate(first);

        var quest = context.Quests.CreateQuest(
            "Quest",
            "Description",
            project
        );

        context.Quests.AssignQuestToMilestone(
            quest,
            first
        );

        context.Quests.ActivateQuest(quest);

        var result =
            context.QuestWorkflow.CompleteQuest(
                quest.Id
            );

        Assert.True(result.MilestoneCompleted);

        Assert.Equal(
            MilestoneStatus.Completed,
            first.Status
        );

        Assert.Equal(
            MilestoneStatus.Active,
            second.Status
        );

        Assert.Equal(
            second.Id,
            result.ActivatedMilestone?.Id
        );
    }

    [Fact]
    public void CompletingBossMilestoneRequirementsUnlocksBossWithoutCompletingMilestone()
    {
        TestContext context = new();

        var project = context.Projects.CreateProject(
            "Project",
            "Description",
            "Reward"
        );

        context.Projects.ActivateProject(project);

        var milestone =
            context.Milestones.CreateMilestone(
                project,
                "Milestone",
                "Description",
                1
            );

        context.Milestones.Activate(milestone);

        var boss = context.Bosses.Create(
            project,
            milestone,
            "Boss",
            "Description",
            true
        );

        var quest = context.Quests.CreateQuest(
            "Quest",
            "Description",
            project
        );

        context.Quests.AssignQuestToMilestone(
            quest,
            milestone
        );

        context.Quests.ActivateQuest(quest);

        var result =
            context.QuestWorkflow.CompleteQuest(
                quest.Id
            );

        Assert.False(result.MilestoneCompleted);

        Assert.Equal(
            MilestoneStatus.Active,
            milestone.Status
        );

        Assert.Equal(
            BossStatus.Available,
            boss.Status
        );

        Assert.Equal(
            boss.Id,
            result.UnlockedBoss?.Id
        );
    }

    [Fact]
    public void DefeatingAvailableBossCompletesMilestoneAndProject()
    {
        TestContext context = new();

        var project = context.Projects.CreateProject(
            "Project",
            "Description",
            "Reward"
        );

        context.Projects.ActivateProject(project);

        var milestone =
            context.Milestones.CreateMilestone(
                project,
                "Milestone",
                "Description",
                1
            );

        context.Milestones.Activate(milestone);

        var boss = context.Bosses.Create(
            project,
            milestone,
            "Boss",
            "Description",
            true
        );

        var quest = context.Quests.CreateQuest(
            "Quest",
            "Description",
            project
        );

        context.Quests.AssignQuestToMilestone(
            quest,
            milestone
        );

        context.Quests.ActivateQuest(quest);

        context.QuestWorkflow.CompleteQuest(
            quest.Id
        );

        var result =
            context.BossWorkflow.Defeat(
                milestone.Id
            );

        Assert.Equal(
            BossStatus.Defeated,
            boss.Status
        );

        Assert.Equal(
            MilestoneStatus.Completed,
            milestone.Status
        );

        Assert.True(result.ProjectCompleted);
    }

    private sealed class TestContext
    {
        private readonly InMemoryStore store = new();

        public ProjectService Projects { get; } =
            new();

        public QuestService Quests { get; } =
            new();

        public MilestoneService Milestones { get; } =
            new();

        public BossService Bosses { get; } =
            new();

        public BookService Books { get; } =
            new();

        public WalletService Wallet { get; } =
            new();

        public QuestWorkflowService QuestWorkflow
        {
            get;
        }

        public BossWorkflowService BossWorkflow
        {
            get;
        }

        public TestContext()
        {
            GameStateService state = new(
                store,
                new HabitService(),
                Projects,
                Quests,
                Milestones,
                Bosses,
                Books,
                Wallet,
                new LevelUp.Domain.Character.Character()
            );

            QuestWorkflow =
                new QuestWorkflowService(
                    Quests,
                    Projects,
                    Milestones,
                    Bosses,
                    state
                );

            BossWorkflow =
                new BossWorkflowService(
                    Bosses,
                    Milestones,
                    Projects,
                    Quests,
                    state
                );
        }
    }

    private sealed class InMemoryStore
        : IGameDataStore
    {
        public GameData? Data { get; private set; }

        public GameData? Load()
        {
            return Data;
        }

        public void Save(GameData gameData)
        {
            Data = gameData;
        }
    }
}