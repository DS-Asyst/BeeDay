using LevelUp.Models;

namespace LevelUp.UI;

public class CharacterScreen
{
    public void Show(Character character)
    {
        Console.Clear();

        Console.WriteLine("================================");
        Console.WriteLine("          CHARACTER");
        Console.WriteLine("================================");
        Console.WriteLine();

        Console.WriteLine($"Name  : {character.Name}");
        Console.WriteLine($"Level : {character.Level}");
        Console.WriteLine(
            $"XP    : {character.Experience}/" +
            $"{character.ExperienceToNextLevel}"
        );

        Console.WriteLine();
        Console.WriteLine("Attributes");
        Console.WriteLine("--------------------------------");

        ShowAttributes(character);
    }

    private static void ShowAttributes(Character character)
    {
        Console.WriteLine(
            $"Strength     : {character.Attributes.Strength.Level}"
        );

        Console.WriteLine(
            $"Intelligence : {character.Attributes.Intelligence.Level}"
        );

        Console.WriteLine(
            $"Vitality     : {character.Attributes.Vitality.Level}"
        );

        Console.WriteLine(
            $"Agility      : {character.Attributes.Agility.Level}"
        );

        Console.WriteLine(
            $"Dexterity    : {character.Attributes.Dexterity.Level}"
        );

        Console.WriteLine(
            $"Luck         : {character.Attributes.Luck.Level}"
        );
    }
}