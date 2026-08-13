using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Common;
using BeeDay.Web.Components.Features.Dashboard.Components;

namespace BeeDay.Web.Tests.Components.Dashboard;

public sealed class ActivityFilterBarTests
{
    [Fact]
    public void RendersSearchTagsToggleAndCreateAction()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>();

        Assert.Contains("placeholder=\"Search\"", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Activity", cut.Markup);
        Assert.DoesNotContain("Add activity", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(">Search activities<", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("total", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll("select"));

        var toggle = cut.Find("button[aria-haspopup='dialog']");
        Assert.Equal("Filter by tags", toggle.GetAttribute("aria-label"));
        Assert.DoesNotContain("Tags", toggle.TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void RendersTheTagsTriggerWithoutAChevronOrExpandIndicator()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>();

        var trigger = cut.Find("button[aria-haspopup='dialog']");
        Assert.Empty(trigger.QuerySelectorAll(".filter-bar__chevron"));

        var filterIcon = trigger.QuerySelector(".beeday-icon--filter");
        Assert.NotNull(filterIcon);
        Assert.Equal("true", filterIcon!.GetAttribute("aria-hidden"));
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
    public async Task TogglesAriaExpandedWithTheTagsMenu()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>();

        var trigger = cut.Find("button[aria-haspopup='dialog']");
        Assert.Null(trigger.GetAttribute("aria-expanded"));

        await trigger.ClickAsync();

        trigger = cut.Find("button[aria-haspopup='dialog']");
        Assert.NotNull(trigger.GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task MarksSelectedAttributeAsChecked()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityFilterBar>(parameters => parameters
            .Add(component => component.SelectedAttributes, [ActivityAttribute.Vitality]));

        await cut.Find("button[aria-haspopup='dialog']").ClickAsync();
        var vitalityCheckbox = cut.Find("input[type='checkbox'][value='Vitality']");

        Assert.True(vitalityCheckbox.HasAttribute("checked"));
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
