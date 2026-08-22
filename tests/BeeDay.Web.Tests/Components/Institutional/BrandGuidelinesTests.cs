using BeeDay.Web.Components.Features.Institutional.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Institutional;

public sealed class BrandGuidelinesTests
{
    [Fact]
    public void RendersTheEditorialShellWithTheRealBrandTaxonomySidebarAndTenSwatches()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandGuidelines>());

        Assert.Equal("Brand guidelines", cut.Find("h1").TextContent.Trim());
        Assert.NotNull(cut.Find("header.beeday-hero"));

        // The real, already-live beeday Brand System taxonomy (ExperienceSystemTopicNav's Brand
        // list) — not an invented one, and not the Duolingo reference taxonomy.
        var topics = cut.FindAll(".experience-system-topic-nav a").Select(a => a.TextContent.Trim()).ToArray();
        Assert.Equal(["Identity", "Wordmark", "Color", "Typography", "Illustration", "Characters", "Writing"], topics);

        Assert.Equal(10, cut.FindAll(".brand-guidelines-swatch").Count);
        Assert.Contains("#5247F9", cut.Markup);

        // No old topic-picker card grid — Brand guidelines is documentation, not a dashboard (§24).
        Assert.Empty(cut.FindAll(".experience-system-topic-grid"));
    }

    [Fact]
    public void HasNoWhiteHeaderFlagsOrContinueButton()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandGuidelines>());

        Assert.Empty(cut.FindAll(".public-header"));
        Assert.Empty(cut.FindAll(".public-language-switcher"));
        Assert.DoesNotContain("Continue to beeday", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ContextualNavigationShowsItsOwnAboutUsFamily()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandGuidelines>());

        var links = cut.FindAll(".editorial-section-nav a").Select(a => a.GetAttribute("href")!).ToArray();
        Assert.Equal(["/mission", "/efficacy", "/brand-guidelines", "/contact"], links);
    }

    [Fact]
    public void UnderPortugueseUiCultureRendersTranslatedHeadingAndTaxonomy()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BrandGuidelines>());

        Assert.Equal("Diretrizes de marca", cut.Find("h1").TextContent.Trim());
        var topics = cut.FindAll(".experience-system-topic-nav a").Select(a => a.TextContent.Trim()).ToArray();
        Assert.Contains("Identidade", topics);
        Assert.Contains("Escrita", topics);
    }
}
