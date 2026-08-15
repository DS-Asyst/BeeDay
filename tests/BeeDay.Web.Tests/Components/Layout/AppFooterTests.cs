using System.Globalization;
using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class AppFooterTests
{
    [Fact]
    public void RendersIdentityAndOnlyRealLinks()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        Assert.Contains("Be Better Every Day", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("a[href='#']"));
        Assert.All(cut.FindAll("a"), link => Assert.StartsWith("https://", link.GetAttribute("href"), StringComparison.Ordinal));
    }

    [Fact]
    public void BrandReusesTheSameAssetVersionedForTheHeader()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        var brandMark = cut.Find("img.app-footer__brand-mark");
        Assert.Equal("/assets/brand/beeday-top-navigation.png", brandMark.GetAttribute("src"));
        Assert.Empty(cut.FindAll(".beeday-brand"));
    }

    [Fact]
    public void Tagline_UnderPortugueseUiCulture_RendersThePortugueseResource()
    {
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            using var context = new BunitContext().WithLocalization();
            var cut = context.Render<AppFooter>();

            Assert.Contains("Seja melhor a cada dia", cut.Markup, StringComparison.Ordinal);
            Assert.DoesNotContain("Be Better Every Day", cut.Markup, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
        }
    }
}
