using LevelUp.Application;
using LevelUp.Domain.Character;
using LevelUp.Domain.Goals;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Goals;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using Xunit;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Tests;

public sealed class Phase7And8Tests
{
    [Theory]
    [InlineData(1, CharacterRank.Apprentice)]
    [InlineData(10, CharacterRank.Adventurer)]
    [InlineData(20, CharacterRank.Disciple)]
    [InlineData(30, CharacterRank.Adept)]
    [InlineData(40, CharacterRank.Specialist)]
    [InlineData(50, CharacterRank.Master)]
    [InlineData(60, CharacterRank.Legend)]
    public void CharacterRank_ShouldFollowProgressionBands(int level, CharacterRank expected)
    {
        Assert.Equal(expected, CharacterRankResolver.Resolve(level));
    }

    [Fact]
    public void Goal_ShouldCompleteWhenTargetIsReached()
    {
        Goal goal = new() { Id = 1 };
        goal.Configure("Ler cem páginas", "Meta de leitura", GoalMetric.PagesRead, 100m);

        bool completed = goal.Evaluate(100m);

        Assert.True(completed);
        Assert.Equal(GoalStatus.Completed, goal.Status);
        Assert.NotNull(goal.CompletedAt);
    }

    [Fact]
    public void GoalService_ShouldEvaluateCompletedQuestGoal()
    {
        ProjectService projects = new();
        var project = projects.CreateProject("Projeto", "Descrição", "Legado");
        QuestService quests = new();
        var quest = quests.CreateQuest("Missão", "Descrição", project);
        quests.ActivateQuest(quest);
        quests.CompleteQuest(quest);

        GoalService goals = new();
        Goal goal = goals.Create("Primeira missão", "Concluir uma missão", GoalMetric.CompletedQuests, 1m);
        GameSession session = CreateSession(projects, quests, goals);

        IReadOnlyList<Goal> completed = goals.EvaluateAll(session);

        Assert.Single(completed);
        Assert.Equal(GoalStatus.Completed, goal.Status);
    }

    [Fact]
    public void GoalService_ShouldUseWalletBalanceForSavingsGoal()
    {
        WalletService wallet = new();
        wallet.AddDeposit(500m, "Reserva", new DateTime(2026, 7, 16));
        GoalService goals = new();
        Goal goal = goals.Create("Reserva inicial", "Guardar dinheiro", GoalMetric.SavingsBalance, 500m);
        GameSession session = CreateSession(wallet: wallet, goals: goals);

        goals.EvaluateAll(session);

        Assert.Equal(GoalStatus.Completed, goal.Status);
    }

    private static GameSession CreateSession(
        ProjectService? projects = null,
        QuestService? quests = null,
        WalletService? wallet = null,
        GoalService? goals = null
    )
    {
        return new GameSession(
            new CharacterModel(),
            new HabitService(),
            projects ?? new ProjectService(),
            quests ?? new QuestService(),
            new MilestoneService(),
            new BossService(),
            new BookService(),
            wallet ?? new WalletService(),
            new AchievementService(),
            goals ?? new GoalService()
        );
    }
}
