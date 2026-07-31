using LevelUp.Domain.Enums;
using LevelUp.Web.Components.DesignSystem.Attributes;

namespace LevelUp.Web.Tests.Components.Attributes;

public sealed class ActivityAttributeComponentTests
{
    [Theory]
    [InlineData(ActivityAttribute.Strength, "Strength")]
    [InlineData(ActivityAttribute.Dexterity, "Dexterity")]
    [InlineData(ActivityAttribute.Intelligence, "Intelligence")]
    [InlineData(ActivityAttribute.Vitality, "Vitality")]
    public void BadgeRendersLabelAndColorOnly(ActivityAttribute attribute, string label)
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeBadge>(parameters => parameters
            .Add(component => component.Attribute, attribute));

        Assert.Contains(label, cut.Find("span.activity-attribute-badge > span").TextContent);
        Assert.Empty(cut.FindAll("svg"));
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
}
