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
        Assert.NotNull(cut.Find("a[href='/experience-system']"));
        Assert.All(cut.FindAll("a:not([href='/experience-system'])"), link =>
            Assert.StartsWith("https://", link.GetAttribute("href"), StringComparison.Ordinal));
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
    public void UnderEnglishUiCulture_RendersEnglishLinksAriaLabelAndCopyright()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        Assert.Equal("beeday links", cut.Find("nav.app-footer__links").GetAttribute("aria-label"));
        Assert.Equal("beeday Experience System", cut.Find("a[href='/experience-system']").TextContent);
        Assert.Equal("Project on GitHub", cut.Find("a[href='https://github.com/tiagoarrigoni/BeeDay']").TextContent);
        Assert.Equal("© 2026 beeday. All rights reserved.", cut.Find(".app-footer__copyright").TextContent);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseLinksAriaLabelAndCopyright()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<AppFooter>());

        Assert.Equal("Links do beeday", cut.Find("nav.app-footer__links").GetAttribute("aria-label"));
        Assert.Equal("beeday Experience System", cut.Find("a[href='/experience-system']").TextContent);
        Assert.Equal("Projeto no GitHub", cut.Find("a[href='https://github.com/tiagoarrigoni/BeeDay']").TextContent);
        Assert.Equal("© 2026 beeday. Todos os direitos reservados.", cut.Find(".app-footer__copyright").TextContent);
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
