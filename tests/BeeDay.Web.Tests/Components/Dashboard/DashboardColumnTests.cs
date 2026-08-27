using BeeDay.Web.Components.Features.Dashboard.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Dashboard;

public sealed class DashboardColumnTests
{
    private static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }

    // EXP32-F003 (Sprint 32.7): focusing a card's own interactive element (e.g. a Habit's score
    // button) must not also draw a second, redundant focus ring around the whole column via
    // :focus-within — every interactive descendant already carries its own focus indication
    // (.habit-card/.activity-card:focus-within, .beeday-icon-toggle:focus-visible).
    [Fact]
    public void DoesNotDeclareFocusWithinOnTheColumnItself()
    {
        var css = File.ReadAllText(Path.Combine(Root, "src", "BeeDay.Web", "Components", "Features", "Dashboard", "Components", "DashboardColumn.razor.css"));

        Assert.DoesNotContain(".dashboard-column:focus-within", css, StringComparison.Ordinal);
    }

    [Fact]
    public void RendersActiveViewByDefaultWithoutLegacyCompletedSection()
    {
        using var context = new BunitContext().WithLocalization();
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
        using var context = new BunitContext().WithLocalization();
        var cut = RenderColumn(context);

        await cut.Find("button.dashboard-column__view-toggle").ClickAsync();

        var toggle = cut.Find("button.dashboard-column__view-toggle");

        Assert.Equal("true", toggle.GetAttribute("aria-pressed"));
        Assert.Contains("Show active", toggle.GetAttribute("aria-label"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Completed item", cut.Markup);
        Assert.DoesNotContain(">Completed<", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Active item", cut.Markup);
    }

    [Theory]
    [InlineData("en-US", "Add Habit")]
    [InlineData("pt-BR", "Adicionar Habit")]
    public void AddButtonAriaLabel_FormatsTheSingularLabelThroughDashboardResources(string culture, string expected)
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => context.Render<DashboardColumn>(parameters => parameters
            .Add(component => component.Title, "Habits")
            .Add(component => component.EmptyTitle, "No habits yet")
            .Add(component => component.EmptyDescription, "Create a habit.")
            .Add(component => component.SingularLabel, "Habit")
            .Add(component => component.ShowCreateButton, true)
            .Add(component => component.ActiveCount, 0)));

        var addButton = cut.Find(".dashboard-column__add");
        Assert.Equal(expected, addButton.GetAttribute("aria-label"));
        Assert.Equal(expected, addButton.GetAttribute("title"));
    }

    // EXP32-F013 follow-up (Sprint 32.10): filtered no-results must offer an actual next action,
    // not just descriptive text — mirrors the canonical Wallet "Clear Filters" pattern
    // (WalletEmptyState) rather than a bespoke one for Dashboard columns.
    [Fact]
    public async Task ShowClearFilterAction_RendersAClearButtonThatInvokesOnClearFilter()
    {
        using var context = new BunitContext().WithLocalization();
        var cleared = false;
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<DashboardColumn>(parameters => parameters
            .Add(component => component.Title, "Habits")
            .Add(component => component.EmptyTitle, "No matches")
            .Add(component => component.EmptyDescription, "Nothing matches your search.")
            .Add(component => component.ActiveCount, 0)
            .Add(component => component.ShowClearFilterAction, true)
            .Add(component => component.OnClearFilter, () => cleared = true)));

        var button = cut.Find(".dashboard-column__clear-filter");
        Assert.Equal("Clear filter", button.TextContent.Trim());

        await button.ClickAsync();

        Assert.True(cleared);
    }

    [Fact]
    public void WhenShowClearFilterActionIsFalse_RendersNoClearButton()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<DashboardColumn>(parameters => parameters
            .Add(component => component.Title, "Habits")
            .Add(component => component.EmptyTitle, "No habits yet")
            .Add(component => component.EmptyDescription, "Create a habit.")
            .Add(component => component.ActiveCount, 0)));

        Assert.Empty(cut.FindAll(".dashboard-column__clear-filter"));
    }

    private static IRenderedComponent<DashboardColumn> RenderColumn(BunitContext context)
    {
        return BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<DashboardColumn>(parameters => parameters
            .Add(component => component.Title, "Tasks")
            .Add(component => component.EmptyTitle, "No tasks yet")
            .Add(component => component.EmptyDescription, "Create a task.")
            .Add(component => component.ActiveStateLabel, "active tasks")
            .Add(component => component.CompletedStateLabel, "completed tasks")
            .Add(component => component.ShowActiveAriaLabel, "Show active tasks")
            .Add(component => component.ShowCompletedAriaLabel, "Show completed tasks")
            .Add(component => component.ActiveCount, 1)
            .Add(component => component.CompletedCount, 1)
            .Add(component => component.ActiveContent, builder => builder.AddContent(0, "Active item"))
            .Add(component => component.CompletedContent, builder => builder.AddContent(0, "Completed item"))));
    }
}
