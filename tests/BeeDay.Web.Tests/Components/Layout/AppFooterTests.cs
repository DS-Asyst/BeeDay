using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class AppFooterTests
{
    [Fact]
    public void RendersIdentityAndOnlyRealLinks()
    {
        using var context = new BunitContext();
        var cut = context.Render<AppFooter>();

        Assert.Contains("Be better every day", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll("a[href='#']"));
        Assert.All(cut.FindAll("a"), link => Assert.StartsWith("https://", link.GetAttribute("href"), StringComparison.Ordinal));
    }
}
