using LevelUp.Domain.Attributes;
using LevelUp.Domain.Character;

namespace LevelUp.Infrastructure.Persistence.Entities;

public sealed class CharacterEntity
{
    public int Id { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; } = CharacterClass.Warrior;
    public int Level { get; set; } = 1;
    public decimal Experience { get; set; }

    public int StrengthLevel { get; set; } = 1;
    public decimal StrengthExperience { get; set; }
    public int IntelligenceLevel { get; set; } = 1;
    public decimal IntelligenceExperience { get; set; }
    public int VitalityLevel { get; set; } = 1;
    public decimal VitalityExperience { get; set; }
    public int AgilityLevel { get; set; } = 1;
    public decimal AgilityExperience { get; set; }
    public int LuckLevel { get; set; } = 1;
    public decimal LuckExperience { get; set; }
    public int DexterityLevel { get; set; } = 1;
    public decimal DexterityExperience { get; set; }
}
