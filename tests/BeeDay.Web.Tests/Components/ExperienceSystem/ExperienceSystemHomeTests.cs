using BeeDay.Web.Components.Features.ExperienceSystem.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.ExperienceSystem;

public sealed class ExperienceSystemHomeTests
{
    [Fact]
    public void EnglishRootLinksToTheThreePillarsWithRealContent()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ExperienceSystemHome>());

        // EPIC 27 Sprint 27.8: "Brand guidelines" is the public title (03_DESIGN_DECISIONS.md §9/§11);
        // "beeday Experience System" is now introductory prose describing what it is, not the H1.
        Assert.Equal("Brand guidelines", cut.Find("h1").TextContent.Trim());
        Assert.Contains("beeday Experience System", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("formalized during EPIC 25", cut.Markup, StringComparison.Ordinal);

        var cards = cut.FindAll(".experience-system-topic-grid__card");
        Assert.Equal(3, cards.Count);
        Assert.Equal(
            ["/experience-system/brand", "/experience-system/ui", "/experience-system/ux"],
            cards.Select(card => card.GetAttribute("href")));
        Assert.Equal(
            ["Brand System", "UI Design System", "UX System"],
            cards.Select(card => card.QuerySelector(".experience-system-topic-grid__title")!.TextContent.Trim()));
    }

    [Fact]
    public void RootUsesTheFullInstitutionalHeroWithABrandContextLockup()
    {
        // The overview/root page is the public "door" into the documentation (03_DESIGN_DECISIONS.md
        // §11) — it gets the full COR8 institutional hero from Sprint 27.3, unlike every individual
        // topic page underneath it (which keep the plain BeeDayPageHeader — see
        // ExperienceSystemBrandPagesTests etc., unaffected by this sprint). The lockup does not use
        // BeeDayBrand's own inverse variant, matching every other institutional hero since Sprint
        // 27.3: BeeDayHero.razor.css forces it to inherit the surface's own paired foreground instead
        // (COR8's white, here), because BeeDayBrand's two fixed colors don't reliably contrast
        // against every COR0-COR9 surface.
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ExperienceSystemHome>());

        var hero = cut.Find("header.beeday-hero");
        Assert.Contains("beeday-surface-cor8", hero.ClassList);
        Assert.Equal("Brand guidelines", cut.Find("header.beeday-hero h1").TextContent.Trim());
        Assert.NotNull(cut.Find(".beeday-hero__brand-context .beeday-brand"));
        Assert.Empty(cut.FindAll(".beeday-page-header"));
    }

    [Fact]
    public void PortugueseRootLocalizesHeadingAndClosingSection()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<ExperienceSystemHome>());

        Assert.Equal("Brand guidelines", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Construído a partir do que já está no ar", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("formalizados durante a EPIC 25", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceDeclaresBothPublicRoutesAnonymousAccessAndPublicLayout()
    {
        var source = File.ReadAllText(Path.Combine(
            ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Features", "ExperienceSystem", "Pages", "ExperienceSystemHome.razor"));

        // /brand-guidelines is the public name/URL (Sprint 27.8); /experience-system keeps working
        // since it predates this Epic and nothing requires breaking it.
        Assert.Contains("@page \"/experience-system\"", source, StringComparison.Ordinal);
        Assert.Contains("@page \"/brand-guidelines\"", source, StringComparison.Ordinal);
        Assert.Contains("@attribute [AllowAnonymous]", source, StringComparison.Ordinal);
        Assert.Contains("@layout BeeDay.Web.Components.Layout.PublicLayout", source, StringComparison.Ordinal);
    }

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }
}
