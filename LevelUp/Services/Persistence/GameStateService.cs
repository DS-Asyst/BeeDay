using LevelUp.Domain;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public sealed class GameStateService
{
    private readonly IGameDataStore dataStore;
    private readonly HabitService habitService;
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly BossService bossService;
    private readonly BookService bookService;
    private readonly WalletService walletService;
    private readonly AchievementService achievementService;
    private readonly CharacterModel character;

    public GameStateService(
        IGameDataStore dataStore,
        HabitService habitService,
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        BossService bossService,
        BookService bookService,
        WalletService walletService,
        CharacterModel character
    )
        : this(
            dataStore,
            habitService,
            projectService,
            questService,
            milestoneService,
            bossService,
            bookService,
            walletService,
            new AchievementService(),
            character
        )
    {
    }

    public GameStateService(
        IGameDataStore dataStore,
        HabitService habitService,
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        BossService bossService,
        BookService bookService,
        WalletService walletService,
        AchievementService achievementService,
        CharacterModel character
    )
    {
        this.dataStore = dataStore;
        this.habitService = habitService;
        this.projectService = projectService;
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.bossService = bossService;
        this.bookService = bookService;
        this.walletService = walletService;
        this.achievementService = achievementService;
        this.character = character;
    }

    public GameData CreateSnapshot()
    {
        return new GameData
        {
            Character = character,
            Habits = habitService.GetAllHabits().ToList(),
            Projects = projectService.GetAllProjects().ToList(),
            Quests = questService.GetAllQuests().ToList(),
            Milestones = milestoneService.GetAll().ToList(),
            Bosses = bossService.GetAll().ToList(),
            Books = bookService.GetAll().ToList(),
            WalletTransactions = walletService.GetAll().ToList(),
            Achievements = achievementService.GetAll().ToList()
        };
    }

    public void Save() => dataStore.Save(CreateSnapshot());
}
