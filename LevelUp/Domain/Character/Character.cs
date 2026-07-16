using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Character;

public class Character : ILevelProgress
{
    public string Name { get; set; } = string.Empty;

    public CharacterClass Class { get; set; } = CharacterClass.Warrior;

    public int Level { get; set; } = 1;

    public decimal Experience { get; set; }

    public PlayerAttributes Attributes { get; set; } = new();

    public decimal ExperienceToNextLevel => Level * 100m;

    public CharacterRank Rank => CharacterRankResolver.Resolve(Level);
}
