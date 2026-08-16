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
}
