using LevelUp.Web.Components.Features.Common;
using LevelUp.Web.Components.Features.Dashboard.Components;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class FilterBarTests
{
    [Fact]
    public void RendersOnlySearchAndCreateControls()
    {
        using var context = new BunitContext();
        var cut = context.Render<FilterBar>();

        Assert.Contains("Search activities or attributes", cut.Markup);
        Assert.Contains("Add activity", cut.Markup);
        Assert.Empty(cut.FindAll("select"));
        Assert.DoesNotContain("total", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpensCreateMenuAndEmitsSelectedActivityType()
    {
        using var context = new BunitContext();
        ActivityType? selectedType = null;

        var cut = context.Render<FilterBar>(parameters => parameters
            .Add(component => component.OnCreate, value => selectedType = value));

        await cut.Find("button[aria-haspopup='menu']").ClickAsync();
        await cut.FindAll("button[role='menuitem']")[0].ClickAsync();

        Assert.Equal(ActivityType.Habit, selectedType);
    }
}
