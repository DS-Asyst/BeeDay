using CharacterModel = LevelUp.Domain.Character.Character;
using LevelUp.Services.Character;

namespace LevelUp.UI;

public class CharacterCreationScreen
{
    private readonly CharacterService characterService;

    public CharacterCreationScreen(
        CharacterService characterService)
    {
        this.characterService = characterService;
    }

    public CharacterModel CreateCharacter()
    {
        Console.Clear();

        Console.WriteLine("=== CRIAÇÃO DO PERSONAGEM ===");
        Console.WriteLine();

        string name;

        do
        {
            Console.Write("Digite o nome do personagem: ");

            name = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine();
                Console.WriteLine(
                    "O nome do personagem é obrigatório."
                );
                Console.WriteLine();
            }

        } while (string.IsNullOrWhiteSpace(name));

        return characterService.CreateCharacter(name);
    }
}
