using LevelUp.Models;
using LevelUp.Services;
using LevelUp.UI;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;


HabitService habitService = new HabitService();
SaveService saveService = new SaveService();
ProgressionService progressionService = new();

InputReader inputReader = new();

CharacterScreen characterScreen = new();

CharacterService characterService =
    new(progressionService);

AttributeService attributeService =
    new(progressionService);

GameData? loadedGame = saveService.LoadGame();

Character character;

if (loadedGame is not null)
{
    character = loadedGame.Character;
    habitService.LoadHabits(loadedGame.Habits);
}
else
{
    CharacterCreationScreen characterCreationScreen =
        new(characterService);

    character =
        characterCreationScreen.CreateCharacter();

    GameData newGameData = new()
    {
        Character = character,
        Habits = habitService.GetAllHabits()
    };

    saveService.SaveGame(newGameData);
}


TrainingScreen trainingScreen = new(
    habitService,
    characterService,
    attributeService,
    saveService,
    inputReader,
    character
);

QuestScreen questScreen = new(inputReader);
ProjectScreen projectScreen = new(inputReader);
GoldScreen goldScreen = new(inputReader);

MainMenuScreen mainMenuScreen = new(
    inputReader,
    characterScreen,
    trainingScreen,
    questScreen,
    projectScreen,
    goldScreen,
    character,
    habitService,
    saveService
);

mainMenuScreen.Show();