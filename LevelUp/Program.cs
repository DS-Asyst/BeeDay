using LevelUp.Models;
using LevelUp.Services;
using LevelUp.UI;

CharacterService characterService = new CharacterService();
HabitService habitService = new HabitService();
SaveService saveService = new SaveService();

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
    character
);

menu.Start();