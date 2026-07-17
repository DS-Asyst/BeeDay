using LevelUp.Domain;
using LevelUp.Domain.Achievements;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Services.Achievements;
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
        var project = context.Projects.CreateProject("Project", "Description");
        context.Projects.ActivateProject(project);
        context.Bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");
        var first = context.Milestones.CreateMilestone(project, "First", "Description", 1);
        var second = context.Milestones.CreateMilestone(project, "Second", "Description", 2);
        context.Milestones.Activate(first);
        var quest = context.Quests.CreateQuest("Quest", "Description", project);
        context.Quests.AssignQuestToMilestone(quest, first);
        context.Quests.ActivateQuest(quest);

        var result = context.QuestWorkflow.CompleteQuest(quest.Id);

        Assert.True(result.MilestoneCompleted);
        Assert.Equal(MilestoneStatus.Completed, first.Status);
        Assert.Equal(MilestoneStatus.Active, second.Status);
    }

    [Fact]
    public void CompletingAllRequirementsUnlocksFinalBoss()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("Project", "Description");
        context.Projects.ActivateProject(project);
        var boss = context.Bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");
        var milestone = context.Milestones.CreateMilestone(project, "Chapter", "Description", 1);
        context.Milestones.Activate(milestone);
        var quest = context.Quests.CreateQuest("Quest", "Description", project);
        context.Quests.AssignQuestToMilestone(quest, milestone);
        context.Quests.ActivateQuest(quest);

        var result = context.QuestWorkflow.CompleteQuest(quest.Id);

        Assert.Equal(BossStatus.Available, boss.Status);
        Assert.Equal(boss.Id, result.UnlockedBoss?.Id);
    }

    [Fact]
    public void DefeatingFinalBossCompletesProjectAndUnlocksAchievement()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("ASP.NET Roadmap", "Description");
        context.Projects.ActivateProject(project);
        var boss = context.Bosses.CreateFinalBoss(
            project,
            "ASP.NET Core",
            "Description",
            "Desenvolvedor"
        );
        var milestone = context.Milestones.CreateMilestone(project, "Chapter", "Description", 1);
        context.Milestones.Activate(milestone);
        var quest = context.Quests.CreateQuest("Quest", "Description", project);
        context.Quests.AssignQuestToMilestone(quest, milestone);
        context.Quests.ActivateQuest(quest);
        context.QuestWorkflow.CompleteQuest(quest.Id);

        var result = context.BossWorkflow.Defeat(project.Id);

        Assert.Equal(BossStatus.Defeated, boss.Status);
        Assert.True(result.ProjectCompleted);
        Assert.Equal(AchievementStatus.Unlocked, result.Achievement.Status);
        Assert.Equal("Desenvolvedor ASP.NET Core", result.Achievement.Name);
    }

    private sealed class TestContext
    {
        private readonly InMemoryStore store = new();
        public ProjectService Projects { get; } = new();
        public QuestService Quests { get; } = new();
        public MilestoneService Milestones { get; } = new();
        public BossService Bosses { get; } = new();
        public AchievementService Achievements { get; } = new();
        public QuestWorkflowService QuestWorkflow { get; }
        public BossWorkflowService BossWorkflow { get; }

        public TestContext()
        {
            GameStateService state = new(
                store,
                new HabitService(),
                Projects,
                Quests,
                Milestones,
                Bosses,
                new BookService(),
                new WalletService(),
                Achievements,
                new LevelUp.Domain.Character.Character()
            );
            QuestWorkflow = new QuestWorkflowService(
                Quests,
                Projects,
                Milestones,
                Bosses,
                state
            );
            BossWorkflow = new BossWorkflowService(
                Bosses,
                Achievements,
                Projects,
                Quests,
                Milestones,
                state
            );
        }
    }

    private sealed class InMemoryStore : IGameDataStore
    {
        public GameData? Data { get; private set; }
        public GameData? Load() => Data;
        public void Save(GameData gameData) => Data = gameData;
    }
}
