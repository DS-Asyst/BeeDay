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
        ConsoleHelper.ShowHeader("Criação do personagem");
        string name = inputReader.ReadRequiredString("Nome do personagem:");
        CharacterClass characterClass = inputReader.ReadSelection(
            "Escolha uma classe:",
            Enum.GetValues<CharacterClass>(),
            value => DisplayText.For(value)
        );

        return characterService.CreateCharacter(name, characterClass);
    }
}
