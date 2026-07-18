using LevelUp.Domain.Character;
using LevelUp.Services.Character;
using LevelUp.UI.Infrastructure;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public sealed class CharacterCreationScreen
{
    private readonly CharacterService characterService;
    private readonly InputReader inputReader;

    public CharacterCreationScreen(
        CharacterService characterService,
        InputReader inputReader
    )
    {
        this.characterService = characterService;
        this.inputReader = inputReader;
    }

    public CharacterModel CreateCharacter()
    {
        ConsoleHelper.ShowHeader("Character Creation");
        string name = inputReader.ReadRequiredString("Character name:");
        CharacterClass characterClass = inputReader.ReadSelection(
            "Choose a class:",
            Enum.GetValues<CharacterClass>(),
            value => DisplayText.For(value)
        );

        return characterService.CreateCharacter(name, characterClass);
    }
}
