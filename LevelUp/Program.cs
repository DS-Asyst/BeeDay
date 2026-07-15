using LevelUp.Domain;
using LevelUp.Services.Character;
using LevelUp.Services.Habits;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.UI;
using CharacterModel = LevelUp.Domain.Character.Character;

Console.OutputEncoding =
    System.Text.Encoding.UTF8;

Console.InputEncoding =
    System.Text.Encoding.UTF8;

HabitService habitService = new();
SaveService saveService = new();
ProgressionService progressionService = new();

InputReader inputReader = new();

CharacterScreen characterScreen = new();

CharacterService characterService =
    new(progressionService);

AttributeService attributeService =
    new(progressionService);

GameData? loadedGame =
    saveService.LoadGame();

CharacterModel character;
ProjectService projectService;
QuestService questService;

if (loadedGame is not null)
{
    character = loadedGame.Character;

    habitService.LoadHabits(
        loadedGame.Habits
    );

    projectService = new ProjectService(
        loadedGame.Projects
    );

    questService = new QuestService(
        loadedGame.Quests
    );
}
else
{
    CharacterCreationScreen characterCreationScreen =
        new(characterService);

    character =
        characterCreationScreen.CreateCharacter();

    projectService =
        new ProjectService();

    questService =
        new QuestService();

    GameData newGameData = new()
    {
        Character = character,

        Habits = habitService
            .GetAllHabits()
            .ToList(),

        Projects = projectService
            .GetAllProjects()
            .ToList(),

        Quests = questService
            .GetAllQuests()
            .ToList()
    };

    saveService.SaveGame(newGameData);
}

TrainingScreen trainingScreen = new(
    habitService,
    characterService,
    attributeService,
    saveService,
    inputReader,
    character,
    projectService,
    questService
);

QuestScreen questScreen = new(
    questService,
    projectService,
    habitService,
    saveService,
    inputReader,
    character
);

ProjectScreen projectScreen = new(
    projectService,
    questService
);

GoldScreen goldScreen = new(
    inputReader
);

MainMenuScreen mainMenuScreen = new(
    inputReader,
    characterScreen,
    trainingScreen,
    questScreen,
    projectScreen,
    goldScreen,
    character,
    habitService,
    projectService,
    questService,
    saveService
);

mainMenuScreen.Show();