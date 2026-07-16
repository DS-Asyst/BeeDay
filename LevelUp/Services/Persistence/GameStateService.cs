using LevelUp.Application;
using LevelUp.Domain;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Goals;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public sealed class GameStateService
{
    private readonly IGameDataStore dataStore;
    private readonly GameSession session;

    public GameStateService(IGameDataStore dataStore, GameSession session)
    {
        this.dataStore = dataStore;
        this.session = session;
    }

    public GameStateService(
        IGameDataStore dataStore, HabitService habits, ProjectService projects,
        QuestService quests, MilestoneService milestones, BossService bosses,
        BookService books, WalletService wallet, CharacterModel character
    ) : this(dataStore, new GameSession(character, habits, projects, quests, milestones, bosses, books, wallet, new AchievementService(), new GoalService()))
    {
    }

    public GameStateService(
        IGameDataStore dataStore, HabitService habits, ProjectService projects,
        QuestService quests, MilestoneService milestones, BossService bosses,
        BookService books, WalletService wallet, AchievementService achievements,
        CharacterModel character
    ) : this(dataStore, new GameSession(character, habits, projects, quests, milestones, bosses, books, wallet, achievements, new GoalService()))
    {
    }

    public GameData CreateSnapshot()
    {
        return new GameData
        {
            SchemaVersion = GameData.CurrentSchemaVersion,
            Character = session.Character,
            Habits = session.Habits.GetAllHabits().ToList(),
            Projects = session.Projects.GetAllProjects().ToList(),
            Quests = session.Quests.GetAllQuests().ToList(),
            Milestones = session.Milestones.GetAll().ToList(),
            Bosses = session.Bosses.GetAll().ToList(),
            Books = session.Books.GetAll().ToList(),
            WalletTransactions = session.Wallet.GetAll().ToList(),
            Achievements = session.Achievements.GetAll().ToList(),
            Goals = session.Goals.GetAll().ToList()
        };
    }

    public void Save() => dataStore.Save(CreateSnapshot());
}
