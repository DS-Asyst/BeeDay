using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Common;
using LevelUp.Web.Components.Features.Dashboard.Components;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class ActivityFilterBarTests
{
    [Fact]
    public void RendersSearchTagsTriggerAndCreateAction()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>();

        Assert.Contains("placeholder=\"Search\"", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Tags", cut.Markup);
        Assert.Contains("Add activity", cut.Markup);
        Assert.DoesNotContain(">Search activities<", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("total", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll("select"));
    }

    [Fact]
    public void RendersTheTagsChevronThroughThePixelIconSystem()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>();

        var chevron = cut.Find("button[aria-haspopup='dialog'] .filter-bar__chevron");
        Assert.Contains("pixel-icon", chevron.ClassList);
        Assert.Equal("true", chevron.GetAttribute("aria-hidden"));
        Assert.DoesNotContain("filter-bar__chevron--open", chevron.ClassList);
    }

    [Fact]
    public async Task OpensTagsMenuAndEmitsSelectedAttribute()
    {
        using var context = new BunitContext();
        ActivityAttribute? selectedAttribute = null;

        var cut = context.Render<ActivityFilterBar>(parameters => parameters
            .Add(component => component.OnAttributeToggle, value => selectedAttribute = value));

        await cut.Find("button[aria-haspopup='dialog']").ClickAsync();
        var strengthCheckbox = cut.Find("input[type='checkbox'][value='Strength']");
        await strengthCheckbox.ChangeAsync(true);

        Assert.Equal(ActivityAttribute.Strength, selectedAttribute);
    }

    [Fact]
    public async Task TogglesTheChevronOpenStateWithTheTagsMenu()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>();

        await cut.Find("button[aria-haspopup='dialog']").ClickAsync();

        var chevron = cut.Find("button[aria-haspopup='dialog'] .filter-bar__chevron");
        Assert.Contains("filter-bar__chevron--open", chevron.ClassList);
    }

    [Fact]
    public async Task MarksSelectedAttributeAsChecked()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>(parameters => parameters
            .Add(component => component.SelectedAttributes, [ActivityAttribute.Wisdom]));

        await cut.Find("button[aria-haspopup='dialog']").ClickAsync();
        var wisdomCheckbox = cut.Find("input[type='checkbox'][value='Wisdom']");

        Assert.True(wisdomCheckbox.HasAttribute("checked"));
    }

    [Fact]
    public async Task ClearsSelectedAttributes()
    {
        using var context = new BunitContext();
        var cleared = false;
        var cut = context.Render<ActivityFilterBar>(parameters => parameters
            .Add(component => component.SelectedAttributes, [ActivityAttribute.Strength])
            .Add(component => component.OnClearAttributes, () => cleared = true));

        await cut.Find("button[aria-haspopup='dialog']").ClickAsync();
        await cut.Find("button.filter-bar__clear").ClickAsync();

        Assert.True(cleared);
    }

    [Fact]
    public async Task OpensCreateMenuAndEmitsSelectedActivityType()
    {
        using var context = new BunitContext();
        ActivityType? selectedType = null;

        var cut = context.Render<ActivityFilterBar>(parameters => parameters
            .Add(component => component.OnCreate, value => selectedType = value));

        await cut.Find("button[aria-haspopup='menu']").ClickAsync();
        await cut.FindAll("button[role='menuitem']")[0].ClickAsync();

        Assert.Equal(ActivityType.Habit, selectedType);
    }
}
