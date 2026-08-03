using BeeDay.Web.Components.DesignSystem.Feedback;
using BeeDay.Web.Components.DesignSystem.Icons;

namespace BeeDay.Web.Tests.Components.Feedback;

public sealed class FeedbackComponentTests
{
    [Fact]
    public void EmptyStateRendersIconTitleDescriptionAndStatusRole()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayEmptyState>(parameters => parameters
            .Add(component => component.Title, "No tasks yet")
            .Add(component => component.Description, "Create a task to get started.")
            .Add(component => component.Icon, PixelIconName.RecurringTask)
            .Add(component => component.Class, "empty-tasks"));

        var root = cut.Find("[role='status']");
        Assert.Contains("empty-tasks", root.ClassList);
        Assert.Equal("No tasks yet", cut.Find(".levelup-empty-state__title").TextContent);
        Assert.Equal("Create a task to get started.", cut.Find(".levelup-empty-state__description").TextContent);
        Assert.Single(cut.FindAll(".levelup-empty-state__icon"));

        var icon = cut.Find(".levelup-empty-state__icon .pixel-icon");
        Assert.Contains("pixel-icon--color-muted", icon.ClassList);
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void LoadingIsHiddenByDefault()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayLoading>();

        Assert.Empty(cut.FindAll("[role='status']"));
    }

    [Fact]
    public void LoadingRendersAccessibleLabelWhenVisible()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayLoading>(parameters => parameters
            .Add(component => component.IsVisible, true)
            .Add(component => component.Label, "Saving..."));

        var status = cut.Find("[role='status']");
        Assert.Equal("Saving...", status.GetAttribute("aria-label"));
        Assert.Contains("Saving...", status.TextContent);
    }

    [Fact]
    public void SkeletonRendersRequestedNumberOfLines()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDaySkeleton>(parameters => parameters
            .Add(component => component.Lines, 5)
            .Add(component => component.Class, "card-placeholder"));

        Assert.Equal(5, cut.FindAll(".levelup-skeleton__line").Count);
        Assert.Contains("card-placeholder", cut.Find(".levelup-skeleton").ClassList);
    }

    [Fact]
    public void DashboardSkeletonRendersFourColumnsAndTwelveCards()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayDashboardSkeleton>();

        Assert.Equal(4, cut.FindAll(".dashboard-skeleton__column").Count);
        Assert.Equal(12, cut.FindAll(".dashboard-skeleton__card").Count);
        Assert.Equal("true", cut.Find("section").GetAttribute("aria-busy"));
    }
}
