using LevelUp.Domain.Enums;
using LevelUp.Web.Components.DesignSystem.Attributes;

namespace LevelUp.Web.Tests.Components.Attributes;

public sealed class ActivityAttributeSelectTests
{
    [Fact]
    public void RendersPlaceholderWhenNoAttributeIsSelected()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute"));

        var trigger = cut.Find(".activity-attribute-select__trigger");
        Assert.Contains("None", trigger.TextContent);
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll(".activity-attribute-select__menu"));
    }

    [Fact]
    public void RendersSelectedAttributeIconAndName()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute")
            .Add(component => component.Value, ActivityAttribute.Strength));

        var trigger = cut.Find(".activity-attribute-select__trigger");
        Assert.Contains("Strength", trigger.TextContent);
        Assert.NotEmpty(trigger.QuerySelectorAll("svg"));
    }

    [Fact]
    public async Task OpensMenuAndListsEveryAttributeWithNoneOption()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute"));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();

        var options = cut.FindAll(".activity-attribute-select__option");
        Assert.Equal(7, options.Count);
        Assert.Equal("None", options[0].TextContent.Trim());
        Assert.All(options.Skip(1), option => Assert.NotEmpty(option.QuerySelectorAll("svg")));
    }

    [Fact]
    public async Task SelectingAnAttributeInvokesValueChangedAndClosesTheMenu()
    {
        using var context = new BunitContext();
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
        using var context = new BunitContext();
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
        using var context = new BunitContext();
        var cut = context.Render<ActivityAttributeSelect>(parameters => parameters
            .Add(component => component.Id, "test-attribute")
            .Add(component => component.Disabled, true));

        await cut.Find(".activity-attribute-select__trigger").ClickAsync();

        Assert.Empty(cut.FindAll(".activity-attribute-select__menu"));
    }
}
