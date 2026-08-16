using BeeDay.Web.Components.Features.Common;
using BeeDay.Web.Components.Features.Dashboard.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Dashboard;

public sealed class ActivityFilterBarTests
{
    [Fact]
    public void RendersSearchAndCreateActionWithoutAttributeFilters()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ActivityFilterBar>());

        Assert.Contains("placeholder=\"Search\"", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Activity", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("select"));
        Assert.Empty(cut.FindAll("button[aria-haspopup='dialog']"));
        Assert.DoesNotContain("Attribute", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("false", cut.Find("button[aria-haspopup='menu']").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task OpensCreateMenuAndEmitsSelectedActivityType()
    {
        using var context = new BunitContext().WithLocalization();
        ActivityType? selectedType = null;

        var cut = context.Render<ActivityFilterBar>(parameters => parameters
            .Add(component => component.OnCreate, value => selectedType = value));

        await cut.Find("button[aria-haspopup='menu']").ClickAsync();
        Assert.Equal("true", cut.Find("button[aria-haspopup='menu']").GetAttribute("aria-expanded"));
        await cut.FindAll("button[role='menuitem']")[0].ClickAsync();

        Assert.Equal(ActivityType.Habit, selectedType);
    }
}
