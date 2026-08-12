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

        Assert.Equal("BEEDAY", cut.Find(".support-drawer__brand-mark .beeday-brand").TextContent.Trim());
        Assert.Contains("DAY", cut.Find(".support-drawer__brand-mark .beeday-brand__accent").TextContent);
        Assert.Equal("GAMIFYING YOUR LIFE", cut.Find(".support-drawer__tagline").TextContent);

        Assert.DoesNotContain("SOCIAL", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll(".support-drawer__social-links"));
    }
}
