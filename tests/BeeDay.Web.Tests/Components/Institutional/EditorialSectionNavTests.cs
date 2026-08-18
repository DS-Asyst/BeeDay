using BeeDay.Web.Components.Features.Institutional;
using BeeDay.Web.Components.Features.Institutional.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Institutional;

public sealed class EditorialSectionNavTests
{
    [Fact]
    public void RendersOnlyItsOwnFamilysLinksWithTheCurrentPageMarked()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<EditorialSectionNav>(parameters => parameters
            .Add(component => component.Section, EditorialSection.AboutUs)
            .Add(component => component.CurrentHref, "/efficacy")));

        var links = cut.FindAll("a").Select(a => a.GetAttribute("href")!).ToArray();
        Assert.Equal(["/mission", "/efficacy", "/brand-guidelines", "/contact"], links);

        var current = cut.Find("a[href='/efficacy']");
        Assert.Equal("page", current.GetAttribute("aria-current"));
        Assert.Contains("editorial-section-nav__link--current", current.ClassList);

        var sibling = cut.Find("a[href='/mission']");
        Assert.Null(sibling.GetAttribute("aria-current"));
    }

    [Theory]
    [InlineData(EditorialSection.Products, new[] { "/beeday", "/beeday-plus" })]
    [InlineData(EditorialSection.Apps, new[] { "/android", "/ios" })]
    [InlineData(EditorialSection.Legal, new[] { "/community-guidelines", "/terms", "/privacy" })]
    public void NeverMixesLinksFromAnotherFamily(EditorialSection section, string[] expectedHrefs)
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<EditorialSectionNav>(parameters => parameters
            .Add(component => component.Section, section)
            .Add(component => component.CurrentHref, expectedHrefs[0])));

        var links = cut.FindAll("a").Select(a => a.GetAttribute("href")!).ToArray();
        Assert.Equal(expectedHrefs, links);
    }

    [Fact]
    public void RendersNothingForASinglePageFamily()
    {
        // Help only has one footer link (FAQs) — no siblings to navigate to.
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<EditorialSectionNav>(parameters => parameters
            .Add(component => component.Section, EditorialSection.Help)
            .Add(component => component.CurrentHref, "/faqs")));

        Assert.Empty(cut.FindAll("nav"));
    }

    [Fact]
    public void UnderPortugueseUiCultureRendersTranslatedLabels()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<EditorialSectionNav>(parameters => parameters
            .Add(component => component.Section, EditorialSection.AboutUs)
            .Add(component => component.CurrentHref, "/mission")));

        Assert.Equal("Missão", cut.Find("a[href='/mission']").TextContent.Trim());
        Assert.Equal("Diretrizes de marca", cut.Find("a[href='/brand-guidelines']").TextContent.Trim());
    }
}
