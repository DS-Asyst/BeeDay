using LevelUp.Domain;
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
using CharacterModel = LevelUp.Domain.Character.Character;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

HabitService habitService = new();
SaveService saveService = new();
ProgressionService progressionService = new();
InputReader inputReader = new();
CharacterScreen characterScreen = new();

CharacterService characterService = new(progressionService);
AttributeService attributeService = new(progressionService);

GameData? loadedGame;

try
{
    loadedGame = saveService.Load();
}
catch (CorruptedSaveException exception)
{
    ConsoleHelper.ShowWarning(
        "O arquivo de salvamento estava corrompido. " +
        $"Um backup foi criado em: {exception.BackupPath}"
    );

    loadedGame = null;
}

CharacterModel character;
ProjectService projectService;
QuestService questService;
MilestoneService milestoneService;
BossService bossService;
BookService bookService;
WalletService walletService;
bool isNewGame = loadedGame is null;

if (loadedGame is not null)
{
    character = loadedGame.Character;
    habitService.LoadHabits(loadedGame.Habits);
    projectService = new ProjectService(loadedGame.Projects);
    questService = new QuestService(loadedGame.Quests);
    milestoneService = new MilestoneService(loadedGame.Milestones);
    bossService = new BossService(loadedGame.Bosses);
    bookService = new BookService(loadedGame.Books);
    walletService = new WalletService(loadedGame.WalletTransactions);
}
else
{
    CharacterCreationScreen characterCreationScreen = new(characterService);
    character = characterCreationScreen.CreateCharacter();
    projectService = new ProjectService();
    questService = new QuestService();
    milestoneService = new MilestoneService();
    bossService = new BossService();
    bookService = new BookService();
    walletService = new WalletService();
}

GameStateService gameStateService = new(
    saveService,
    habitService,
    projectService,
    questService,
    milestoneService,
    bossService,
    bookService,
    walletService,
    character
);

if (isNewGame)
{
    gameStateService.Save();
}

QuestWorkflowService questWorkflowService = new(
    questService,
    projectService,
    milestoneService,
    bossService,
    gameStateService
);

ProjectWorkflowService projectWorkflowService = new(
    projectService,
    questService,
    milestoneService,
    bossService,
    gameStateService
);

MilestoneWorkflowService milestoneWorkflowService = new(
    milestoneService,
    questService,
    bossService,
    gameStateService
);


BossWorkflowService bossWorkflowService = new(
    bossService,
    milestoneService,
    projectService,
    questService,
    gameStateService
);

TrainingScreen trainingScreen = new(
    habitService,
    characterService,
    attributeService,
    inputReader,
    character,
    gameStateService
);

QuestScreen questScreen = new(
    questService,
    projectService,
    inputReader,
    gameStateService,
    questWorkflowService,
    milestoneService
);

MilestoneScreen milestoneScreen = new(
    projectService,
    questService,
    milestoneService,
    bossService,
    milestoneWorkflowService,
    bossWorkflowService,
    gameStateService,
    inputReader
);

ProjectScreen projectScreen = new(
    projectService,
    questService,
    inputReader,
    gameStateService,
    projectWorkflowService,
    milestoneService,
    milestoneScreen
);


WalletScreen walletScreen = new(
    walletService,
    gameStateService,
    inputReader
);

BackpackScreen backpackScreen = new(
    inputReader,
    walletScreen
);

ReadingWorkflowService readingWorkflowService = new(
    bookService,
    characterService,
    character,
    gameStateService
);

LibraryScreen libraryScreen = new(
    bookService,
    readingWorkflowService,
    gameStateService,
    inputReader
);

DiaryScreen diaryScreen = new(
    inputReader,
    trainingScreen,
    questScreen,
    projectScreen,
    milestoneScreen
);

MainMenuScreen mainMenuScreen = new(
    inputReader,
    characterScreen,
    diaryScreen,
    libraryScreen,
    backpackScreen,
    character,
    gameStateService
);

mainMenuScreen.Show();
