using BeeDay.Web.Components.DesignSystem;
using BeeDay.Web.Components.Features.Institutional.Components;

namespace BeeDay.Web.Tests.Components.Institutional;

public sealed class InstitutionalPageShellTests
{
    [Fact]
    public void RendersHeroWithBrandContextTitleAndDescription()
    {
        using var context = new BunitContext();
        var cut = context.Render<InstitutionalPageShell>(parameters => parameters
            .Add(component => component.PageContext, "Mission")
            .Add(component => component.Title, "Our mission")
            .Add(component => component.Description, "Be better every day.")
            .Add(component => component.Surface, BeeDayPaletteToken.Cor0)
            .AddChildContent("<p>Body content.</p>"));

        Assert.Equal("Our mission", cut.Find("h1").TextContent);
        Assert.Contains("Mission", cut.Find(".institutional-hero__context-label").TextContent);
        Assert.NotNull(cut.Find(".beeday-hero__brand-context .beeday-brand"));
        Assert.Contains("Be better every day.", cut.Find(".beeday-hero__subtitle").TextContent);
        Assert.Contains("Body content.", cut.Find(".institutional-page__body").InnerHtml);
        Assert.Contains("beeday-surface-cor0", cut.Find("header.beeday-hero").ClassList);
    }

    [Theory]
    [InlineData(BeeDayPaletteToken.Cor0)]
    [InlineData(BeeDayPaletteToken.Cor3)]
    [InlineData(BeeDayPaletteToken.Cor4)]
    [InlineData(BeeDayPaletteToken.Cor8)]
    public void UsesTheNonWhiteBackgroundIconRegardlessOfSurfaceWhileTextStillInheritsTheHeroForeground(BeeDayPaletteToken surface)
    {
        // Every institutional hero surface (Cor0-Cor9, the unused white Cor6 excepted) is non-white,
        // so the shell always passes OnDarkSurface="true" to BeeDayBrand to select the approved
        // non-white-background bee icon (Sprint 29.1). That also applies BeeDayBrand's own
        // beeday-brand--inverse class, but its fixed white TEXT color never actually renders:
        // BeeDayHero.razor.css forces the lockup text to inherit the hero's own paired foreground for
        // every COR0-COR9 surface instead — see the CSS-source assertion in VisualFoundationTests for
        // that half of the contract, since bUnit cannot resolve computed CSS cascade/specificity.
        using var context = new BunitContext();
        var cut = context.Render<InstitutionalPageShell>(parameters => parameters
            .Add(component => component.PageContext, "Mission")
            .Add(component => component.Title, "Our mission")
            .Add(component => component.Surface, surface));

        var brand = cut.Find(".beeday-hero__brand-context .beeday-brand");
        Assert.Contains("beeday-brand--inverse", brand.ClassList);
        var icon = brand.QuerySelector(".beeday-brand__icon");
        Assert.NotNull(icon);
        Assert.Equal("/assets/brand/bee-color-neutral.png", icon.GetAttribute("src"));
    }

    [Fact]
    public void RendersHeroAndBodyAsSiblingsWithoutAWrappingArticle()
    {
        // Sprint 29.2: nesting BeeDayHero inside a single wrapping <article class="institutional-page">
        // let polish.css's shared reading-width rule (which matches only direct children of
        // .beeday-main) cap the hero's own full-bleed background to 72rem, rendering it as a small
        // centered card instead of a full-width colored band. The hero and body are now separate root
        // elements — mirroring ExperienceSystemPage's existing sibling structure.
        using var context = new BunitContext();
        var cut = context.Render<InstitutionalPageShell>(parameters => parameters
            .Add(component => component.PageContext, "Mission")
            .Add(component => component.Title, "Our mission")
            .AddChildContent("<p>Body content.</p>"));

        Assert.Empty(cut.FindAll("article.institutional-page"));
        Assert.NotNull(cut.Find("header.beeday-hero"));
        Assert.NotNull(cut.Find("article.institutional-page__body"));
    }

    [Fact]
    public void RendersPrimaryActionInTheHeroWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<InstitutionalPageShell>(parameters => parameters
            .Add(component => component.PageContext, "beeday")
            .Add(component => component.Title, "beeday")
            .Add(component => component.PrimaryAction, builder => builder.AddMarkupContent(0, "<a class=\"beeday-button\" href=\"/profile/create\">Get started</a>")));

        var cta = cut.Find(".beeday-hero__primary-action a");
        Assert.Equal("/profile/create", cta.GetAttribute("href"));
    }

    [Fact]
    public void OmitsPrimaryActionWrapperWhenNotSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<InstitutionalPageShell>(parameters => parameters
            .Add(component => component.PageContext, "Mission")
            .Add(component => component.Title, "Our mission"));

        Assert.Empty(cut.FindAll(".beeday-hero__primary-action"));
    }
}
