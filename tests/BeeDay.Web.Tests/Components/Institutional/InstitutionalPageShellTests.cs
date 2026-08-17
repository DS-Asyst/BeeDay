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
    public void NeverAppliesBeeDayBrandsOwnInverseVariantRegardlessOfSurface(BeeDayPaletteToken surface)
    {
        // BeeDayBrand only models two fixed colors (brand-primary purple / white), neither of which
        // reliably contrasts against every COR0-COR9 surface (brand-primary purple text failed WCAG
        // color-contrast against COR3/COR4 in a real axe-core E2E run). The shell instead lets
        // BeeDayHero.razor.css force the lockup text to inherit the hero's own paired foreground —
        // see the CSS-source assertion in VisualFoundationTests for that half of the contract.
        using var context = new BunitContext();
        var cut = context.Render<InstitutionalPageShell>(parameters => parameters
            .Add(component => component.PageContext, "Mission")
            .Add(component => component.Title, "Our mission")
            .Add(component => component.Surface, surface));

        var brand = cut.Find(".beeday-hero__brand-context .beeday-brand");
        Assert.DoesNotContain("beeday-brand--inverse", brand.ClassList);
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
