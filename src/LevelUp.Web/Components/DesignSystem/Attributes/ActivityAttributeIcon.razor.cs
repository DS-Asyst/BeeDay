using LevelUp.Domain.Enums;
using LevelUp.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Attributes;

public partial class ActivityAttributeIcon
{
    [Parameter] public ActivityAttribute? Attribute { get; set; }
    [Parameter] public PixelIconSize Size { get; set; } = PixelIconSize.Small;
    [Parameter] public bool Decorative { get; set; } = true;
    [Parameter] public string? Class { get; set; }

    private string Label => Attribute?.ToString() ?? string.Empty;

    private string CssClass => string.Join(' ', new[]
    {
        "activity-attribute-icon",
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private PixelIconName IconName => Attribute switch
    {
        ActivityAttribute.Strength => PixelIconName.Strength,
        ActivityAttribute.Dexterity => PixelIconName.Dexterity,
        ActivityAttribute.Intelligence => PixelIconName.Intelligence,
        ActivityAttribute.Wisdom => PixelIconName.Wisdom,
        ActivityAttribute.Vitality => PixelIconName.Vitality,
        ActivityAttribute.Charisma => PixelIconName.Charisma,
        _ => PixelIconName.Warning
    };

    private PixelIconColor Color => Attribute switch
    {
        ActivityAttribute.Strength => PixelIconColor.Strength,
        ActivityAttribute.Dexterity => PixelIconColor.Dexterity,
        ActivityAttribute.Intelligence => PixelIconColor.Intelligence,
        ActivityAttribute.Wisdom => PixelIconColor.Wisdom,
        ActivityAttribute.Vitality => PixelIconColor.Vitality,
        ActivityAttribute.Charisma => PixelIconColor.Charisma,
        _ => PixelIconColor.Warning
    };
}
