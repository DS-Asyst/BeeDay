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

        var wordmark = cut.Find(".support-drawer__brand-mark .beeday-brand__wordmark");
        Assert.Equal("BeeDay", wordmark.GetAttribute("alt"));
        Assert.Equal("/beeday-wordmark.png", wordmark.GetAttribute("src"));
        Assert.Equal("GAMIFYING YOUR LIFE", cut.Find(".support-drawer__tagline").TextContent);

        Assert.DoesNotContain("SOCIAL", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll(".support-drawer__social-links"));
    }
}
