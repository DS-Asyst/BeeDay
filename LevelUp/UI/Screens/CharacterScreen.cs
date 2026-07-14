using LevelUp.Models;
using LevelUp.UI.Components.Character;
using Spectre.Console;

namespace LevelUp.UI;

public class CharacterScreen
{
    public void Show(Character character)
    {
        ConsoleHelper.ShowHeader("Character");

        CharacterCard characterCard = new(character);
        AttributeTable attributeTable = new(character.Attributes);

        AnsiConsole.Write(characterCard.Build());

        AnsiConsole.WriteLine();

        AnsiConsole.Write(attributeTable.Build());
    }
}