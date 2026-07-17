using LevelUp.Domain;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Quests;
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

public sealed class ProgressionAutomationTests
{
    [Fact]
    public void CompletingQuest_ShouldActivateNextQuestInSameChapter()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("Project", "Description");
        context.Projects.ActivateProject(project);
        context.Bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");
        var chapter = context.Milestones.CreateMilestone(
            project,
            "Chapter",
            "Description",
            1
        );
        context.Milestones.Activate(chapter);

        var first = context.Quests.CreateQuest("First", "Description", project);
        var second = context.Quests.CreateQuest("Second", "Description", project);
        context.Quests.AssignQuestToMilestone(first, chapter);
        context.Quests.AssignQuestToMilestone(second, chapter);
        context.Quests.ActivateQuest(first);

        var result = context.QuestWorkflow.CompleteQuest(first.Id);

        Assert.False(result.MilestoneCompleted);
        Assert.Equal(QuestStatus.Active, second.Status);
        Assert.Equal(second.Id, result.ActivatedQuest?.Id);
    }

    [Fact]
    public void CompletingChapter_ShouldActivateFirstQuestOfNextChapter()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("Project", "Description");
        context.Projects.ActivateProject(project);
        context.Bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");
        var firstChapter = context.Milestones.CreateMilestone(
            project,
            "First chapter",
            "Description",
            1
        );
        var secondChapter = context.Milestones.CreateMilestone(
            project,
            "Second chapter",
            "Description",
            2
        );
        context.Milestones.Activate(firstChapter);

        var firstQuest = context.Quests.CreateQuest("First", "Description", project);
        var secondQuest = context.Quests.CreateQuest("Second", "Description", project);
        context.Quests.AssignQuestToMilestone(firstQuest, firstChapter);
        context.Quests.AssignQuestToMilestone(secondQuest, secondChapter);
        context.Quests.ActivateQuest(firstQuest);

        var result = context.QuestWorkflow.CompleteQuest(firstQuest.Id);

        Assert.True(result.MilestoneCompleted);
        Assert.Equal(MilestoneStatus.Active, secondChapter.Status);
        Assert.Equal(QuestStatus.Active, secondQuest.Status);
        Assert.Equal(secondQuest.Id, result.ActivatedQuest?.Id);
    }

    [Fact]
    public void CompletingFinalChapter_ShouldNotActivateRemainingQuestInCompletedChapter()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("Project", "Description");
        context.Projects.ActivateProject(project);
        context.Bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");
        var chapter = context.Milestones.CreateMilestone(
            project,
            "Chapter",
            "Description",
            1,
            requiredCompletedQuests: 1
        );
        context.Milestones.Activate(chapter);

        var first = context.Quests.CreateQuest("First", "Description", project);
        var second = context.Quests.CreateQuest("Second", "Description", project);
        context.Quests.AssignQuestToMilestone(first, chapter);
        context.Quests.AssignQuestToMilestone(second, chapter);
        context.Quests.ActivateQuest(first);

        var result = context.QuestWorkflow.CompleteQuest(first.Id);

        Assert.True(result.MilestoneCompleted);
        Assert.Equal(QuestStatus.Created, second.Status);
        Assert.Null(result.ActivatedQuest);
    }

    [Fact]
    public void ActivatingProject_ShouldActivateFirstChapterAndItsFirstQuest()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("Project", "Description");
        context.Bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");
        var chapter = context.Milestones.CreateMilestone(
            project,
            "Chapter",
            "Description",
            1
        );
        var quest = context.Quests.CreateQuest("Quest", "Description", project);
        context.Quests.AssignQuestToMilestone(quest, chapter);

        context.ProjectWorkflow.ActivateProject(project.Id);

        Assert.Equal(MilestoneStatus.Active, chapter.Status);
        Assert.Equal(QuestStatus.Active, quest.Status);
    }

    [Fact]
    public void DeletingCompletedQuest_ShouldActivateFirstCreatedQuestInSameChapter()
    {
        TestContext context = new();
        var project = context.Projects.CreateProject("Project", "Description");
        context.Projects.ActivateProject(project);
        var chapter = context.Milestones.CreateMilestone(project, "Chapter", "Description", 1);
        context.Milestones.Activate(chapter);
        var completed = context.Quests.CreateQuest("Completed", "Description", project);
        var next = context.Quests.CreateQuest("Next", "Description", project);
        context.Quests.AssignQuestToMilestone(completed, chapter);
        context.Quests.AssignQuestToMilestone(next, chapter);
        context.Quests.ActivateQuest(completed);
        context.Quests.CompleteQuest(completed);

        bool deleted = context.Quests.DeleteQuest(completed.Id);

        Assert.True(deleted);
        Assert.Equal(QuestStatus.Active, next.Status);
    }

    private sealed class TestContext
    {
        private readonly InMemoryStore store = new();

        public ProjectService Projects { get; } = new();
        public QuestService Quests { get; } = new();
        public MilestoneService Milestones { get; } = new();
        public BossService Bosses { get; } = new();
        public QuestWorkflowService QuestWorkflow { get; }
        public ProjectWorkflowService ProjectWorkflow { get; }

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
                new AchievementService(),
                new LevelUp.Domain.Character.Character()
            );

            QuestWorkflow = new QuestWorkflowService(
                Quests,
                Projects,
                Milestones,
                Bosses,
                state
            );
            ProjectWorkflow = new ProjectWorkflowService(
                Projects,
                Quests,
                Milestones,
                Bosses,
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
