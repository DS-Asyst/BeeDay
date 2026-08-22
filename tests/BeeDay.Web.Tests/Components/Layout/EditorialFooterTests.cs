using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class EditorialFooterTests
{
    [Fact]
    public void RendersBackToTopBuyMeACoffeeAndCopyrightWithNoColumnsOrLanguageSwitcher()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<EditorialFooter>());

        var backToTop = cut.Find(".editorial-footer__back-to-top");
        Assert.Equal("button", backToTop.TagName.ToLowerInvariant());
        Assert.Equal("Back to top", backToTop.GetAttribute("aria-label"));

        var coffee = cut.Find(".editorial-footer__coffee");
        Assert.Equal("BUY ME A COFFEE", coffee.TextContent.Trim());
        Assert.Equal("/buy-me-a-coffee", coffee.GetAttribute("href"));

        Assert.Contains("© 2026 beeday. All rights reserved.", cut.Find(".editorial-footer__copyright").TextContent);

        // No AppFooter-style groups, mascot, or language switcher — this is the minimal editorial
        // footer (Sprint 29.4 §26), not the Home footer.
        Assert.Empty(cut.FindAll(".app-footer__groups"));
        Assert.Empty(cut.FindAll(".public-language-switcher"));
        Assert.Empty(cut.FindAll("img"));
    }

    [Fact]
    public void ScrollToTopButtonHasNoTypeSubmitToAvoidAccidentalFormSubmission()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<EditorialFooter>());

        Assert.Equal("button", cut.Find(".editorial-footer__back-to-top").GetAttribute("type"));
    }

    [Fact]
    public void UnderPortugueseUiCultureRendersTranslatedCopyrightAndBackToTopLabel()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<EditorialFooter>());

        Assert.Equal("Voltar ao topo", cut.Find(".editorial-footer__back-to-top").GetAttribute("aria-label"));
        Assert.Contains("© 2026 beeday. Todos os direitos reservados.", cut.Find(".editorial-footer__copyright").TextContent);
        // BUY ME A COFFEE is fixed, official copy in both cultures (Sprint 29.4 prompt §29).
        Assert.Equal("BUY ME A COFFEE", cut.Find(".editorial-footer__coffee").TextContent.Trim());
    }
}
