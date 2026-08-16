using BeeDay.Web.Components.Features.Brand.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Brand;

public sealed class TypographyGuidelinesTests
{
    [Fact]
    public void EnglishPagePresentsLiveBrandAndProductTypographyWithUsageGuidance()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<TypographyGuidelines>());

        Assert.Equal("Typography with purpose", cut.Find("h1").TextContent.Trim());
        Assert.Equal("beeday", cut.Find(".brand-typography__display-sample").TextContent.Trim());
        Assert.Contains("Coiny is our display voice", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Nunito is the product foundation", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(7, cut.FindAll(".brand-typography__role").Count);
        Assert.Equal(2, cut.FindAll(".brand-typography__guidance-card").Count);
        Assert.DoesNotContain(">BeeDay<", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(">BEEDAY<", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortuguesePageLocalizesContentAndPreservesAccentedDisplaySample()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<TypographyGuidelines>());

        Assert.Equal("Tipografia com propósito", cut.Find("h1").TextContent.Trim());
        Assert.Equal("Pequenas ações, grandes conquistas.", cut.Find(".brand-typography__accented-sample").TextContent.Trim());
        Assert.Contains("Nunito é a base do produto", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Não use Coiny em textos", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceDeclaresPublicRouteAnonymousAccessAndPublicLayout()
    {
        var source = File.ReadAllText(Path.Combine(
            ResolveRepoRoot(),
            "src",
            "BeeDay.Web",
            "Components",
            "Features",
            "Brand",
            "Pages",
            "TypographyGuidelines.razor"));

        Assert.Contains("@page \"/brand/typography\"", source, StringComparison.Ordinal);
        Assert.Contains("@page \"/experience-system/brand/typography\"", source, StringComparison.Ordinal);
        Assert.Contains("@attribute [AllowAnonymous]", source, StringComparison.Ordinal);
        Assert.Contains("@layout BeeDay.Web.Components.Layout.PublicLayout", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PageRendersExperienceSystemPillarAndTopicNavigationForBothRoutes()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<TypographyGuidelines>());

        Assert.Equal("page", cut.Find(".experience-system-pillar-nav a[href='/experience-system/brand']").GetAttribute("aria-current"));
        Assert.Equal("page", cut.Find(".experience-system-topic-nav a[href='/experience-system/brand/typography']").GetAttribute("aria-current"));
        Assert.Equal("Typography with purpose", cut.Find("h1").TextContent.Trim());
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
