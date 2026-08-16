using BeeDay.Web.Components.Features.ExperienceSystem.Pages.Brand;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.ExperienceSystem;

public sealed class ExperienceSystemBrandPagesTests
{
    [Fact]
    public void OverviewListsAllSevenBrandTopicsIncludingTypographyCompatibilityRoute()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandOverview>());

        Assert.Equal("Brand System", cut.Find("h1").TextContent.Trim());
        var cards = cut.FindAll(".experience-system-topic-grid__card");
        Assert.Equal(7, cards.Count);
        Assert.Contains("/experience-system/brand/typography", cards.Select(card => card.GetAttribute("href")));
        Assert.Equal(
            ["Identity", "Wordmark", "Color", "Typography", "Illustration", "Characters", "Writing"],
            cards.Select(card => card.QuerySelector(".experience-system-topic-grid__title")!.TextContent.Trim()));
    }

    [Fact]
    public void IdentityPageStatesTheBrandContractAndNavigationIsWiredToThePillar()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandIdentity>());

        Assert.Equal("Identity", cut.Find("h1").TextContent.Trim());
        Assert.Contains("#5247F9", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Brand identity ≠ technical identity", cut.Markup, StringComparison.Ordinal);

        Assert.Equal("page", cut.Find(".experience-system-pillar-nav a[href='/experience-system/brand']").GetAttribute("aria-current"));
        Assert.Equal("page", cut.Find(".experience-system-topic-nav a[href='/experience-system/brand/identity']").GetAttribute("aria-current"));
        Assert.Equal(7, cut.FindAll(".experience-system-topic-nav a").Count);
    }

    [Fact]
    public void PortugueseIdentityPageLocalizesTheContract()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BrandIdentity>());

        Assert.Equal("Identidade", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Identidade de marca ≠ identidade técnica", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void WordmarkPageDescribesBothCanonicalRepresentations()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandWordmark>());

        Assert.Equal("Wordmark", cut.Find("h1").TextContent.Trim());
        Assert.Contains("BeeDayBrand", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("866×288", cut.Markup, StringComparison.Ordinal);
        Assert.NotNull(cut.Find("img.brand-wordmark__lockup"));
        Assert.NotEmpty(cut.FindAll(".beeday-brand"));
    }

    [Fact]
    public void ColorPageListsTheSevenCategoriesAndBrandPrimaryStates()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandColor>());

        Assert.Equal("Color", cut.Find("h1").TextContent.Trim());
        Assert.Equal(7, cut.FindAll(".experience-system-table tbody tr").Count);
        Assert.Equal(5, cut.FindAll(".experience-system-swatch").Count);
        Assert.Contains("#FFD326", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseColorPageLocalizesCategoryNames()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BrandColor>());

        Assert.Equal("Cor", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Nem todo literal é dívida", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void IllustrationPageCoversShapeLanguageAndAccessibility()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandIllustration>());

        Assert.Equal("Illustration", cut.Find("h1").TextContent.Trim());
        Assert.Contains("4.6 MB", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("hidden from assistive technology", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void CharactersPageListsTheBeeAndSixSupportingCharacters()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandCharacters>());

        Assert.Equal("Characters", cut.Find("h1").TextContent.Trim());
        Assert.Equal(6, cut.FindAll(".experience-system-table tbody tr").Count);
        Assert.Contains("Orange fox", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Anthropomorphic flower", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseCharactersPageLocalizesDescriptions()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BrandCharacters>());

        Assert.Equal("Personagens", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Raposa alaranjada", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void WritingPageCoversVoiceToneStyleAndGlossary()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandWriting>());

        Assert.Equal("Writing", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Clear", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Pendência", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("belongs to exactly one Project", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseWritingPageLocalizesGlossaryMeanings()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BrandWriting>());

        Assert.Equal("Escrita", cut.Find("h1").TextContent.Trim());
        Assert.Contains("pertence a exatamente um Project", cut.Markup, StringComparison.Ordinal);
    }
}
