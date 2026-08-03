using LevelUp.Web.Components.Layout;

namespace LevelUp.Web.Tests.Components.Layout;

public sealed class AccountSidePanelTests
{
    [Fact]
    public void RendersInstitutionalBrandingInsteadOfSocialMedia()
    {
        using var context = new BunitContext();
        var cut = context.Render<AccountSidePanel>(parameters => parameters
            .Add(component => component.IsOpen, true));

        Assert.Contains("LEVEL", cut.Find(".support-drawer__brand-level").TextContent, StringComparison.Ordinal);
        Assert.Contains("UP", cut.Find(".support-drawer__brand-up").TextContent, StringComparison.Ordinal);
        Assert.Equal("GAMIFYING YOUR LIFE", cut.Find(".support-drawer__tagline").TextContent);

        Assert.DoesNotContain("SOCIAL", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll(".support-drawer__social-links"));
    }
}
