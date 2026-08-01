namespace LevelUp.Domain.Enums;

/// <summary>
/// Optional productivity classifier for tagging which focus area an activity builds
/// (used for filtering, organization, and progress insights). Not an RPG character stat,
/// ability score, or distributable point — it carries no combat, equipment, or class semantics.
/// </summary>
public enum ActivityAttribute
{
    Strength = 1,
    Dexterity = 2,
    Intelligence = 3,
    Vitality = 4
}
