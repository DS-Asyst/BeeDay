using LevelUp.Domain.Enums;
using LevelUp.Web.Components.DesignSystem.Attributes;

namespace LevelUp.Web.Tests.Components.Attributes;

public sealed class ActivityAttributeComponentTests
{
    [Theory]
    [InlineData(ActivityAttribute.Strength, "attribute-strength.svg", "Strength")]
    [InlineData(ActivityAttribute.Dexterity, "attribute-dexterity.svg", "Dexterity")]
    [InlineData(ActivityAttribute.Intelligence, "attribute-intelligence.svg", "Intelligence")]
    [InlineData(ActivityAttribute.Wisdom, "attribute-wisdom.svg", "Wisdom")]
    [InlineData(ActivityAttribute.Vitality, "attribute-vitality.svg", "Vitality")]
    [InlineData(ActivityAttribute.Charisma, "attribute-charisma.svg", "Charisma")]
    public void BadgeRendersOfficialIconAndLabel(ActivityAttribute attribute, string iconFile, string label)
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeBadge>(parameters => parameters
            .Add(component => component.Attribute, attribute));

        Assert.Contains(label, cut.Find("span.activity-attribute-badge > span").TextContent);
        Assert.Contains(iconFile, cut.Find("image").GetAttribute("href"));
        Assert.Contains($"activity-attribute-badge--{label.ToLowerInvariant()}", cut.Find("span").ClassList);
    }

    [Fact]
    public void BadgeRendersNothingWhenAttributeIsNull()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeBadge>();

        Assert.Empty(cut.Markup);
    }

    [Fact]
    public void IconCanExposeAccessibleLabel()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeIcon>(parameters => parameters
            .Add(component => component.Attribute, ActivityAttribute.Vitality)
            .Add(component => component.Decorative, false));

        Assert.Equal("img", cut.Find("svg").GetAttribute("role"));
        Assert.Equal("Vitality", cut.Find("title").TextContent);
    }
}
