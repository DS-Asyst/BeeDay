using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Character;

public class Character : ILevelProgress
{
    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } = 1;

    public decimal Experience { get; set; } = 0m;

    public PlayerAttributes Attributes { get; set; } = new();

    public decimal ExperienceToNextLevel => Level * 100m;
}