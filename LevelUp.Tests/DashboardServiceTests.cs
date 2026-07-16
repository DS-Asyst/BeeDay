using LevelUp.Application;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Achievements;
using LevelUp.Services.Analytics;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Goals;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using Xunit;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public void GetSnapshot_ShouldAggregateSessionData()
    {
        CharacterModel character = new() { Level = 10, Experience = 25m };
        ProjectService projects = new();
        var project = projects.CreateProject("Projeto", "Descrição", "Legado");
        projects.ActivateProject(project);
        QuestService quests = new();
        var quest = quests.CreateQuest("Missão", "Descrição", project);
        quests.ActivateQuest(quest);
        WalletService wallet = new();
        wallet.AddDeposit(100m, "Reserva", new DateTime(2026, 7, 1));

        GameSession session = new(
            character,
            new HabitService(),
            projects,
            quests,
            new MilestoneService(),
            new BossService(),
            new BookService(),
            wallet,
            new AchievementService(),
            new GoalService()
        );

        DashboardSnapshot snapshot = new DashboardService(session)
            .GetSnapshot(new DateTime(2026, 7, 16));

        Assert.Equal(1, snapshot.ActiveProjects);
        Assert.Equal(1, snapshot.ActiveQuests);
        Assert.Equal(100m, snapshot.WalletBalance);
        Assert.Equal(100m, snapshot.WalletMonthResult);
    }
}
