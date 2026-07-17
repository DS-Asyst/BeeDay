using LevelUp.Domain.Character;
using LevelUp.Domain.Rewards;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Character;

public sealed class CharacterService
{
    private readonly ProgressionService progressionService;

    public CharacterService(ProgressionService progressionService)
    {
        this.progressionService = progressionService;
    }

    public CharacterModel CreateCharacter(
        string name,
        CharacterClass characterClass
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new CharacterModel
        {
            Name = name.Trim(),
            Class = characterClass
        };
    }

    public CharacterModel CreateCharacter(string name)
    {
        return CreateCharacter(name, CharacterClass.Warrior);
    }

    public void AddExperience(CharacterModel character, decimal experience)
    {
        ArgumentNullException.ThrowIfNull(character);
        character.ApplyReward(new Reward(Experience: experience));
    }
}
