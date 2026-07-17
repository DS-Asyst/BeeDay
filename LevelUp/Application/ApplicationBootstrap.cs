using LevelUp.Domain;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Character;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using LevelUp.Services.Workflows;
using LevelUp.UI;
using LevelUp.UI.Flows.Quests;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Application;

public static class ApplicationBootstrap
{
    public static MainMenuScreen Build(IGameDataStore dataStore)
    {
        ArgumentNullException.ThrowIfNull(dataStore);

        HabitService habitService = new();
        IGameDataStore persistence = dataStore;
        ProgressionService progressionService = new();
        InputReader inputReader = new();
        CharacterService characterService = new(progressionService);
        AttributeService attributeService = new(progressionService);

        GameData? loadedGame;
        try
        {
            loadedGame = persistence.Load();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Não foi possível carregar os dados do banco SQLite.",
                exception
            );
        }

        CharacterModel character;
        ProjectService projects;
        QuestService quests;
        MilestoneService milestones;
        BossService bosses;
        BookService books;
        WalletService wallet;
        AchievementService achievements;
        bool isNewGame = loadedGame is null;

        if (loadedGame is not null)
        {
            character = loadedGame.Character;
            habitService.LoadHabits(loadedGame.Habits);
            projects = new ProjectService(loadedGame.Projects);
            quests = new QuestService(loadedGame.Quests);
            milestones = new MilestoneService(loadedGame.Milestones);
            bosses = new BossService(loadedGame.Bosses);
            books = new BookService(loadedGame.Books);
            wallet = new WalletService(loadedGame.WalletTransactions, loadedGame.WalletTags);
            achievements = new AchievementService(loadedGame.Achievements);
        }
        else
        {
            CharacterCreationScreen creation = new(characterService, inputReader);
            character = creation.CreateCharacter();
            projects = new ProjectService();
            quests = new QuestService();
            milestones = new MilestoneService();
            bosses = new BossService();
            books = new BookService();
            wallet = new WalletService();
            achievements = new AchievementService();
        }

        GameSession session = new(
            character,
            habitService,
            projects,
            quests,
            milestones,
            bosses,
            books,
            wallet,
            achievements,
            loadedGame?.SaveRevision ?? 0,
            loadedGame?.LastSavedAt
        );

        GameStateService state = new(persistence, session);
        if (isNewGame)
        {
            state.Save();
        }

        QuestWorkflowService questWorkflow = new(quests, projects, milestones, bosses, state, character);
        ProjectWorkflowService projectWorkflow = new(projects, quests, milestones, bosses, state);
        MilestoneWorkflowService milestoneWorkflow = new(milestones, quests, bosses, projects, state);
        BossWorkflowService bossWorkflow = new(bosses, achievements, projects, quests, milestones, state);

        TrainingScreen training = new(habitService, characterService, attributeService, inputReader, character, state);
        QuestSelectionFlow questSelection = new(projects, milestones, inputReader);
        QuestScreen questScreen = new(quests, projects, inputReader, state, questWorkflow, questSelection);
        MilestoneScreen milestoneScreen = new(quests, milestones, milestoneWorkflow, state, inputReader);
        ProjectScreen projectScreen = new(projects, quests, inputReader, state, projectWorkflow, milestones, milestoneScreen, bosses, bossWorkflow);
        WalletScreen walletScreen = new(wallet, state, inputReader);
        ReadingWorkflowService readingWorkflow = new(books, achievements, character, state);
        LibraryScreen library = new(books, readingWorkflow, state, inputReader);
        InventoryScreen inventory = new(inputReader, library, walletScreen);
        DiaryScreen diary = new(inputReader, training, questScreen, projectScreen);
        CharacterScreen characterScreen = new(inputReader, achievements);
        SettingsScreen settings = new(inputReader, state);

        return new MainMenuScreen(
            inputReader,
            characterScreen,
            diary,
            inventory,
            settings,
            character,
            state
        );
    }
}
