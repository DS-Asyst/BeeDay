using LevelUp.Web.Components.Features.Dashboard.Components;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class DashboardColumnTests
{
    [Fact]
    public void RendersActiveViewByDefaultWithoutLegacyCompletedSection()
    {
        using var context = new BunitContext();
        var cut = RenderColumn(context);

        var toggle = cut.Find("button.dashboard-column__view-toggle");

        Assert.Equal("false", toggle.GetAttribute("aria-pressed"));
        Assert.Contains("Show completed", toggle.GetAttribute("aria-label"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Active item", cut.Markup);
        Assert.DoesNotContain(">Active<", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Show &gt;", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dashboard-column__completed", cut.Markup);
    }

    [Fact]
    public async Task TogglesBetweenActiveAndCompletedContentFromHeader()
    {
        using var context = new BunitContext();
        var cut = RenderColumn(context);

        await cut.Find("button.dashboard-column__view-toggle").ClickAsync();

        var toggle = cut.Find("button.dashboard-column__view-toggle");

        Assert.Equal("true", toggle.GetAttribute("aria-pressed"));
        Assert.Contains("Show active", toggle.GetAttribute("aria-label"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Completed item", cut.Markup);
        Assert.DoesNotContain(">Completed<", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Active item", cut.Markup);
    }

    private static IRenderedComponent<DashboardColumn> RenderColumn(BunitContext context)
    {
        return context.Render<DashboardColumn>(parameters => parameters
            .Add(component => component.Title, "Tasks")
            .Add(component => component.EmptyLabel, "tasks")
            .Add(component => component.EmptyTitle, "No tasks yet")
            .Add(component => component.EmptyDescription, "Create a task.")
            .Add(component => component.ActiveCount, 1)
            .Add(component => component.CompletedCount, 1)
            .Add(component => component.ActiveContent, builder => builder.AddContent(0, "Active item"))
            .Add(component => component.CompletedContent, builder => builder.AddContent(0, "Completed item")));
    }
}
