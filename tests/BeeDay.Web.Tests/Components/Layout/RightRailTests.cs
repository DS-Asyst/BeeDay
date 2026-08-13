using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class RightRailTests
{
    [Fact]
    public void RendersLoadingWithoutPresentingZeroAsRealData()
    {
        using var context = new BunitContext();

        var cut = context.Render<RightRail>();

        var aside = cut.Find("aside.right-rail");
        Assert.Equal("Progress and status", aside.GetAttribute("aria-label"));
        Assert.Equal("true", aside.QuerySelector("[aria-label='Loading experience']")?.GetAttribute("aria-busy"));
        Assert.DoesNotContain("0 XP", aside.TextContent);
    }
}
