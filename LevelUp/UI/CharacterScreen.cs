using LevelUp.Models;
using LevelUp.UI.Components.Cards;
using Spectre.Console;

namespace LevelUp.UI;

public class CharacterScreen
{
    public void Show(Character character)
    {
        ConsoleHelper.ShowHeader("Character");

        CharacterCard characterCard = new(character);

        AnsiConsole.Write(characterCard.Build());
    }
}