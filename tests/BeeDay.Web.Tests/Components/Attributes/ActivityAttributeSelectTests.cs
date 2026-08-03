using BeeDay.Domain.Enums;
using BeeDay.Web.Components.DesignSystem.Attributes;

namespace BeeDay.Web.Tests.Components.Attributes;

public sealed class ActivityAttributeSelectTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void RendersPlaceholderWhenNoAttributeIsSelected()
    {
        using var context = CreateContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute"));

        var trigger = cut.Find(".activity-attribute-select__trigger");
        Assert.Contains("None", trigger.TextContent);
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll(".activity-attribute-select__menu"));
    }

    [Fact]
    public void RendersSelectedAttributeBadgeAndName()
    {
        using var context = CreateContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute")
            .Add(component => component.Value, ActivityAttribute.Strength));

        var trigger = cut.Find(".activity-attribute-select__trigger");
        Assert.Contains("Strength", trigger.TextContent);
        Assert.NotEmpty(trigger.QuerySelectorAll(".activity-attribute-badge"));
    }

    [Fact]
    public async Task OpensMenuAndListsEveryAttributeWithNoneOption()
    {
        using var context = CreateContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute"));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();

        var options = cut.FindAll(".activity-attribute-select__option");
        Assert.Equal(5, options.Count);
        Assert.Equal("None", options[0].TextContent.Trim());
        Assert.All(options.Skip(1), option => Assert.NotEmpty(option.QuerySelectorAll(".activity-attribute-badge")));
    }

    [Fact]
    public async Task SelectingAnAttributeInvokesValueChangedAndClosesTheMenu()
    {
        using var context = CreateContext();
        ActivityAttribute? selected = null;
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute")
            .Add(component => component.ValueChanged, (ActivityAttribute? value) => selected = value));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();
        await cut.FindAll(".activity-attribute-select__option")[1].ClickAsync();

        Assert.Equal(ActivityAttribute.Strength, selected);
        Assert.Empty(cut.FindAll(".activity-attribute-select__menu"));
    }

    [Fact]
    public async Task SelectingNoneInvokesValueChangedWithNull()
    {
        using var context = CreateContext();
        ActivityAttribute? selected = ActivityAttribute.Strength;
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute")
            .Add(component => component.Value, ActivityAttribute.Strength)
            .Add(component => component.ValueChanged, (ActivityAttribute? value) => selected = value));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();
        await cut.FindAll(".activity-attribute-select__option")[0].ClickAsync();

        Assert.Null(selected);
    }

    [Fact]
    public async Task DisabledTriggerDoesNotOpenTheMenu()
    {
        using var context = CreateContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute")
            .Add(component => component.Disabled, true));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();

        Assert.Empty(cut.FindAll(".activity-attribute-select__menu"));
    }

    [Fact]
    public async Task PlacesMenuBelowByDefaultWhenViewportSpaceIsSufficient()
    {
        using var context = CreateContext();
        context.JSInterop.SetupModule("./js/activity-attribute-select.js")
            .Setup<AttributeSelectGeometry>("measureGeometry", _ => true)
            .SetResult(new AttributeSelectGeometry(
                TriggerTop: 100, TriggerBottom: 130, TriggerLeft: 40, TriggerWidth: 200,
                MenuHeight: 180, ViewportHeight: 800));

        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute"));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();

        var menu = cut.Find(".activity-attribute-select__menu");
        Assert.DoesNotContain("activity-attribute-select__menu--flip-up", menu.ClassList);
        Assert.DoesNotContain("activity-attribute-select__menu--measuring", menu.ClassList);
        Assert.Contains("top:136px", menu.GetAttribute("style"));
    }

    [Fact]
    public async Task FlipsMenuAboveWhenViewportSpaceBelowIsInsufficient()
    {
        using var context = CreateContext();
        context.JSInterop.SetupModule("./js/activity-attribute-select.js")
            .Setup<AttributeSelectGeometry>("measureGeometry", _ => true)
            .SetResult(new AttributeSelectGeometry(
                TriggerTop: 750, TriggerBottom: 780, TriggerLeft: 40, TriggerWidth: 200,
                MenuHeight: 180, ViewportHeight: 800));

        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute"));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();

        var menu = cut.Find(".activity-attribute-select__menu");
        Assert.Contains("activity-attribute-select__menu--flip-up", menu.ClassList);
        Assert.DoesNotContain("activity-attribute-select__menu--measuring", menu.ClassList);
    }
}
