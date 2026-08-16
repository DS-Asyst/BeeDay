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

        Assert.Equal("beeday Experience System", cut.Find("h1").TextContent.Trim());
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
    public void PortugueseRootLocalizesHeadingAndClosingSection()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<ExperienceSystemHome>());

        Assert.Equal("beeday Experience System", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Construído a partir do que já está no ar", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("formalizados durante a EPIC 25", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceDeclaresPublicRouteAnonymousAccessAndPublicLayout()
    {
        var source = File.ReadAllText(Path.Combine(
            ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Features", "ExperienceSystem", "Pages", "ExperienceSystemHome.razor"));

        Assert.Contains("@page \"/experience-system\"", source, StringComparison.Ordinal);
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
