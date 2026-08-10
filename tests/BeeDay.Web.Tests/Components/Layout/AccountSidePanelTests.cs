using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class AccountSidePanelTests
{
    [Fact]
    public void RendersInstitutionalBrandingInsteadOfSocialMedia()
    {
        using var context = new BunitContext();
        var cut = context.Render<AccountSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        Assert.Contains("BEE", cut.Find(".support-drawer__brand-bee").TextContent, StringComparison.Ordinal);
        Assert.Contains("DAY", cut.Find(".support-drawer__brand-day").TextContent, StringComparison.Ordinal);
        Assert.Equal("GAMIFYING YOUR LIFE", cut.Find(".support-drawer__tagline").TextContent);

        Assert.DoesNotContain("SOCIAL", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll(".support-drawer__social-links"));
    }
}
