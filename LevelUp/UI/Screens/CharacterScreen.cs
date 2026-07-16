using CharacterModel = LevelUp.Domain.Character.Character;
using LevelUp.UI.Components.Character;
using Spectre.Console;

namespace LevelUp.UI;

public class CharacterScreen
{
    public void Show(CharacterModel character)
    {
        ConsoleHelper.ShowHeader("Personagem");

        CharacterCard characterCard = new(character);
        AttributeTable attributeTable = new(character.Attributes);

        AnsiConsole.Write(characterCard.Build());

        AnsiConsole.WriteLine();

        AnsiConsole.Write(attributeTable.Build());
    }
}
