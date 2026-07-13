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
    character = characterService.CreateCharacter("Tiago");
}

ConsoleMenu menu = new ConsoleMenu(
    characterService,
    habitService,
    saveService,
    character,
    attributeService
);

menu.Start();