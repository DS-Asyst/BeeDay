using LevelUp.Models;
using LevelUp.Services;
using LevelUp.UI;


HabitService habitService = new HabitService();
SaveService saveService = new SaveService();
ProgressionService progressionService = new();

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

ConsoleMenu menu = new ConsoleMenu(
    characterService,
    habitService,
    saveService,
    character,
    attributeService
);

menu.Start();