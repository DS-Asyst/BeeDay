using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Character;

public sealed class CharacterService
{
    private readonly ProgressionService progressionService;

    public CharacterService(
        ProgressionService progressionService
    )
    {
        this.progressionService = progressionService;
    }

    public CharacterModel CreateCharacter(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CharacterModel
        {
            Name = name.Trim()
        };
    }

    public void AddExperience(
        CharacterModel character,
        decimal experience
    )
    {
        ArgumentNullException.ThrowIfNull(character);

        progressionService.AddExperience(
            character,
            experience
        );
    }
}