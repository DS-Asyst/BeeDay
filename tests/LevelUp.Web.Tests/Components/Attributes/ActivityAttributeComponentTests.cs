using LevelUp.Domain.Enums;
using LevelUp.Web.Components.DesignSystem.Attributes;

namespace LevelUp.Web.Tests.Components.Attributes;

public sealed class ActivityAttributeComponentTests
{
    [Theory]
    [InlineData(ActivityAttribute.Strength, "attribute-strength", "Strength")]
    [InlineData(ActivityAttribute.Dexterity, "attribute-dexterity", "Dexterity")]
    [InlineData(ActivityAttribute.Intelligence, "attribute-intelligence", "Intelligence")]
    [InlineData(ActivityAttribute.Wisdom, "attribute-wisdom", "Wisdom")]
    [InlineData(ActivityAttribute.Vitality, "attribute-vitality", "Vitality")]
    [InlineData(ActivityAttribute.Charisma, "attribute-charisma", "Charisma")]
    public void BadgeRendersOfficialIconAndLabel(ActivityAttribute attribute, string symbolId, string label)
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeBadge>(parameters => parameters
            .Add(component => component.Attribute, attribute));

        Assert.Contains(label, cut.Find("span.activity-attribute-badge > span").TextContent);
        Assert.EndsWith($"#{symbolId}", cut.Find("use").GetAttribute("href"));
        Assert.Contains($"activity-attribute-badge--{label.ToLowerInvariant()}", cut.Find("span").ClassList);
        Assert.Equal($"{label} activity attribute", cut.Find("span.activity-attribute-badge").GetAttribute("title"));
        Assert.Equal($"{label} activity attribute", cut.Find("span.activity-attribute-badge").GetAttribute("aria-label"));
    }

    [Fact]
    public void BadgeRendersNothingWhenAttributeIsNull()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeBadge>();

        Assert.Empty(cut.Markup);
    }

    [Fact]
    public void IconCombinesBaseAndCustomClasses()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeIcon>(parameters => parameters
            .Add(component => component.Attribute, ActivityAttribute.Strength)
            .Add(component => component.Class, "custom-icon-class"));

        var icon = cut.Find("svg");
        Assert.Contains("activity-attribute-icon", icon.ClassList);
        Assert.Contains("custom-icon-class", icon.ClassList);
    }

    [Fact]
    public void IconCanExposeAccessibleLabel()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeIcon>(parameters => parameters
            .Add(component => component.Attribute, ActivityAttribute.Vitality)
            .Add(component => component.Decorative, false));

        Assert.Equal("img", cut.Find("svg").GetAttribute("role"));
        Assert.Equal("Vitality", cut.Find("svg").GetAttribute("aria-label"));
    }
}
