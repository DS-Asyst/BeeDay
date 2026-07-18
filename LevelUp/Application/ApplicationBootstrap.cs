using LevelUp.Domain;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Character;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Tasks;
using LevelUp.Services.Todos;
using LevelUp.Services.Wallet;
using LevelUp.Services.Workflows;
using LevelUp.UI;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Application;

public static class ApplicationBootstrap
{
    public static MainMenuScreen Build(IGameDataStore dataStore)
    {
        ArgumentNullException.ThrowIfNull(dataStore);

        ProgressionService progression = new();
        InputReader input = new();
        CharacterService characterService = new(progression);
        AttributeService attributeService = new(progression);

        GameData? loaded;
        try { loaded = dataStore.Load(); }
        catch (Exception exception)
        {
            throw new InvalidOperationException("The SQLite game data could not be loaded.", exception);
        }

        CharacterModel character;
        HabitService habits;
        TaskService tasks;
        ProjectService projects;
        ProjectTodoService todos;
        MilestoneService milestones;
        BossService bosses;
        BookService books;
        WalletService wallet;
        AchievementService achievements;
        bool isNewGame = loaded is null;

        if (loaded is null)
        {
            character = new CharacterCreationScreen(characterService, input).CreateCharacter();
            habits = new HabitService();
            tasks = new TaskService();
            projects = new ProjectService();
            todos = new ProjectTodoService();
            milestones = new MilestoneService();
            bosses = new BossService();
            books = new BookService();
            wallet = new WalletService();
            achievements = new AchievementService();
        }
        else
        {
            character = loaded.Character;
            habits = new HabitService();
            habits.LoadHabits(loaded.Habits);
            tasks = new TaskService(loaded.Tasks);
            projects = new ProjectService(loaded.Projects);
            todos = new ProjectTodoService(loaded.Todos);
            milestones = new MilestoneService(loaded.Milestones);
            bosses = new BossService(loaded.Bosses);
            books = new BookService(loaded.Books);
            wallet = new WalletService(loaded.WalletTransactions, loaded.WalletTags);
            achievements = new AchievementService(loaded.Achievements);
        }

        GameSession session = new(character, habits, tasks, projects, todos, milestones, bosses, books, wallet, achievements, loaded?.SaveRevision ?? 0, loaded?.LastSavedAt);
        GameStateService state = new(dataStore, session);
        if (isNewGame) state.Save();

        HabitScreen habitScreen = new(habits, characterService, attributeService, input, character, state);
        TaskScreen taskScreen = new(tasks, state, input);
        TodoScreen todoScreen = new(todos, input, state);
        ProjectScreen projectScreen = new(projects, todos, input, state);
        WalletScreen walletScreen = new(wallet, state, input);
        ReadingWorkflowService readingWorkflow = new(books, achievements, character, state);
        LibraryScreen library = new(books, readingWorkflow, state, input);
        InventoryScreen inventory = new(input, library, walletScreen);
        DiaryScreen diary = new(input, habitScreen, taskScreen, todoScreen, projectScreen);
        CharacterScreen characterScreen = new(input, achievements);
        SettingsScreen settings = new(input, state);

        return new MainMenuScreen(input, characterScreen, diary, inventory, settings, character, state);
    }
}
