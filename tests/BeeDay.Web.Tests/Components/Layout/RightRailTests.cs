using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class RightRailTests
{
    [Fact]
    public void RendersAsAnEmptyStructuralLandmarkWithNoSimulatedContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<RightRail>();

        var aside = cut.Find("aside.right-rail");
        Assert.Equal("Context and progress", aside.GetAttribute("aria-label"));
        Assert.Equal(string.Empty, aside.TextContent.Trim());
        Assert.Empty(aside.Children);
    }
}
