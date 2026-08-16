using BeeDay.Web.Components.Features.ExperienceSystem.Pages.Ux;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.ExperienceSystem;

public sealed class ExperienceSystemUxPagesTests
{
    [Fact]
    public void OverviewListsAllFiveUxTopics()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UxOverview>());

        Assert.Equal("UX System", cut.Find("h1").TextContent.Trim());
        Assert.Equal(
            ["Accessibility", "Responsive", "Localization", "Motion", "Performance"],
            cut.FindAll(".experience-system-topic-grid__title").Select(el => el.TextContent.Trim()));
    }

    [Fact]
    public void AccessibilityPageStatesAriaFocusAndContrastGuaranteesWithDisclaimer()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UxAccessibility>());

        Assert.Equal("Accessibility", cut.Find("h1").TextContent.Trim());
        Assert.Contains("8.69:1", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("not WCAG conformance", cut.Markup, StringComparison.Ordinal);

        Assert.Equal("page", cut.Find(".experience-system-pillar-nav a[href='/experience-system/ux']").GetAttribute("aria-current"));
        Assert.Equal("page", cut.Find(".experience-system-topic-nav a[href='/experience-system/ux/accessibility']").GetAttribute("aria-current"));
    }

    [Fact]
    public void PortugueseAccessibilityPageLocalizesTheDisclaimer()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UxAccessibility>());

        Assert.Equal("Acessibilidade", cut.Find("h1").TextContent.Trim());
        Assert.Contains("não conformidade WCAG", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ResponsivePageListsRegressionViewportsAndShellBreakpoint()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UxResponsive>());

        Assert.Equal("Responsive", cut.Find("h1").TextContent.Trim());
        Assert.Contains("390 and 430px", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("1200px", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalizationPageStatesCulturePrecedenceAndNeverTranslatedValues()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UxLocalization>());

        Assert.Equal("Localization", cut.Find("h1").TextContent.Trim());
        Assert.Contains("An explicit culture cookie always wins", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(3, cut.FindAll(".experience-system-page__body ul li").Count);
    }

    [Fact]
    public void PortugueseLocalizationPageLocalizesNeverTranslatedList()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UxLocalization>());

        Assert.Equal("Localização", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Conteúdo gerado pelo usuário", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void MotionPageListsSixTaxonomyCategories()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UxMotion>());

        Assert.Equal("Motion", cut.Find("h1").TextContent.Trim());
        Assert.Equal(6, cut.FindAll(".experience-system-page__body ul li").Count);
        Assert.Contains("23 of 31 production stylesheets", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseMotionPageUsesTheEstablishedMovimentoTerm()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UxMotion>());

        Assert.Equal("Movimento", cut.Find("h1").TextContent.Trim());
        Assert.Contains("23 de 31 stylesheets de produção", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PerformancePageStatesLoadingDisciplineAndKnownGaps()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UxPerformance>());

        Assert.Equal("Performance", cut.Find("h1").TextContent.Trim());
        Assert.Contains("high fetch priority", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("no responsive image pipeline", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Not a performance budget", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortuguesePerformancePageLocalizesKnownGaps()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UxPerformance>());

        Assert.Equal("Performance", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Não há pipeline de imagem responsiva", cut.Markup, StringComparison.Ordinal);
    }
}
