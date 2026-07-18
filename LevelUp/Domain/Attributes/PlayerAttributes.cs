using System;
using System.Collections.Generic;
using System.Text;


namespace LevelUp.Domain.Attributes;

public class PlayerAttributes
{
    public AttributeProgress Strength { get; set; } = new();

    public AttributeProgress Intelligence { get; set; } = new();

    public AttributeProgress Vitality { get; set; } = new();

    public AttributeProgress Agility { get; set; } = new();

    public AttributeProgress Luck { get; set; } = new();

    public AttributeProgress Dexterity { get; set; } = new();

    public AttributeProgress GetAttribute(AttributeType attributeType)
    {
        return attributeType switch
        {
            AttributeType.Strength => Strength,
            AttributeType.Intelligence => Intelligence,
            AttributeType.Vitality => Vitality,
            AttributeType.Agility => Agility,
            AttributeType.Luck => Luck,
            AttributeType.Dexterity => Dexterity,

            _ => throw new ArgumentOutOfRangeException(
                nameof(attributeType),
                "Invalid attribute.")
        };
    }
}
