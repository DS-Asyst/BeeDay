using BeeDay.Web.Components.DesignSystem.Feedback;
using BeeDay.Web.Components.DesignSystem.Icons;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Feedback;

public sealed class FeedbackComponentTests
{
    [Fact]
    public void EmptyStateRendersIconTitleDescriptionAndStatusRole()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayEmptyState>(parameters => parameters
            .Add(component => component.Title, "No tasks yet")
            .Add(component => component.Description, "Create a task to get started.")
            .Add(component => component.Icon, BeeDayIconName.RecurringTask)
            .Add(component => component.Class, "empty-tasks"));

        var root = cut.Find("[role='status']");
        Assert.Contains("empty-tasks", root.ClassList);
        Assert.Equal("No tasks yet", cut.Find(".beeday-empty-state__title").TextContent);
        Assert.Equal("Create a task to get started.", cut.Find(".beeday-empty-state__description").TextContent);
        Assert.Single(cut.FindAll(".beeday-empty-state__icon"));

        var icon = cut.Find(".beeday-empty-state__icon .beeday-icon");
        Assert.Contains("beeday-icon--color-muted", icon.ClassList);
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void LoadingIsHiddenByDefault()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayLoading>();

        Assert.Empty(cut.FindAll("[role='status']"));
    }

    [Fact]
    public void LoadingRendersAccessibleLabelWhenVisible()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayLoading>(parameters => parameters
            .Add(component => component.IsVisible, true)
            .Add(component => component.Label, "Saving..."));

        var status = cut.Find("[role='status']");
        Assert.Equal("Saving...", status.GetAttribute("aria-label"));
        Assert.Contains("Saving...", status.TextContent);
    }

    [Fact]
    public void UnderEnglishUiCulture_DefaultsUnsetLabelToEnglish()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BeeDayLoading>(parameters => parameters
            .Add(component => component.IsVisible, true)));

        Assert.Equal("Saving changes...", cut.Find("[role='status']").GetAttribute("aria-label"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_DefaultsUnsetLabelToPortuguese()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BeeDayLoading>(parameters => parameters
            .Add(component => component.IsVisible, true)));

        Assert.Equal("Salvando alterações...", cut.Find("[role='status']").GetAttribute("aria-label"));
    }

    // EPIC 30 Sprint 30.23 (BD30-F065): before BeeDayErrorBoundary existed, an unhandled exception
    // anywhere in a page's render tree tore down the entire circuit — no in-app fallback existed at
    // all. This proves the branded fallback renders instead of the exception propagating, and that
    // it replaces (not merely supplements) the original content.
    [Fact]
    public void ErrorBoundary_WhenChildContentThrows_RendersTheBrandedFallbackInsteadOfCrashing()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BeeDayErrorBoundary>(parameters => parameters
            .Add(component => component.ChildContent, builder =>
                throw new InvalidOperationException("Simulated render-time failure."))));

        Assert.Equal("Something went wrong", cut.Find(".beeday-empty-state__title").TextContent);
        Assert.Equal(
            "This page ran into an unexpected problem. Reloading usually fixes it.",
            cut.Find(".beeday-empty-state__description").TextContent);
        Assert.Contains("Reload page", cut.Find(".beeday-error-boundary button").TextContent);
        Assert.DoesNotContain("Simulated render-time failure", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ErrorBoundary_WithNoException_RendersChildContentUnchanged()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = context.Render<BeeDayErrorBoundary>(parameters => parameters
            .Add(component => component.ChildContent, builder =>
            {
                builder.OpenElement(0, "p");
                builder.AddContent(1, "normal content");
                builder.CloseElement();
            }));

        Assert.Equal("normal content", cut.Find("p").TextContent);
        Assert.Empty(cut.FindAll(".beeday-error-boundary"));
    }

    [Fact]
    public void SkeletonRendersRequestedNumberOfLines()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDaySkeleton>(parameters => parameters
            .Add(component => component.Lines, 5)
            .Add(component => component.Class, "card-placeholder"));

        Assert.Equal(5, cut.FindAll(".beeday-skeleton__line").Count);
        Assert.Contains("card-placeholder", cut.Find(".beeday-skeleton").ClassList);
    }

    [Fact]
    public void DashboardSkeletonRendersFourColumnsAndTwelveCards()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayDashboardSkeleton>();

        Assert.Equal(4, cut.FindAll(".dashboard-skeleton__column").Count);
        Assert.Equal(12, cut.FindAll(".dashboard-skeleton__card").Count);
        Assert.Equal("true", cut.Find("section").GetAttribute("aria-busy"));
    }
}
