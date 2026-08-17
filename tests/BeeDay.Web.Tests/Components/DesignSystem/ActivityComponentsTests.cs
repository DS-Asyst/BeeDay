using BeeDay.Web.Components.Features.Dashboard.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class ActivityComponentsTests
{
    [Fact]
    public void ActivityCard_RendersSharedVisualContract()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task"));

        Assert.Contains("activity-card--task", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Read chapter", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ActivityCard_ProjectVariant_RendersTheMiniBeeInsteadOfTheOldIconAndPreservesItsMeta()
    {
        // EPIC 27 Sprint 27.10: the Project rail indicator is a cropped presentation of the
        // existing "how beeday works" illustration (no new bee generated — see project-bee.png)
        // in place of the old BeeDayIconName.Project folder icon. The progress Meta text next to
        // it is untouched.
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Kitchen remodel")
            .Add(component => component.Variant, "project")
            .Add(component => component.Meta, "In progress · 42%"));

        var image = cut.Find(".activity-card__project-status");
        Assert.Equal("img", image.TagName, ignoreCase: true);
        Assert.Equal("/assets/dashboard/project-bee.png", image.GetAttribute("src"));
        Assert.False(string.IsNullOrWhiteSpace(image.GetAttribute("alt")));
        Assert.Empty(cut.FindAll(".activity-card__rail .beeday-icon"));
        Assert.Equal("In progress · 42%", cut.Find(".activity-card__meta").TextContent);
    }
}
