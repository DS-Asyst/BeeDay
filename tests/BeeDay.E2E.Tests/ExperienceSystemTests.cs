using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class ExperienceSystemTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Theory]
    [InlineData("/experience-system", "beeday Experience System")]
    [InlineData("/experience-system/brand", "Brand System")]
    [InlineData("/experience-system/brand/identity", "Identity")]
    [InlineData("/experience-system/brand/wordmark", "Wordmark")]
    [InlineData("/experience-system/brand/color", "Color")]
    [InlineData("/experience-system/brand/typography", "Typography with purpose")]
    [InlineData("/experience-system/brand/illustration", "Illustration")]
    [InlineData("/experience-system/brand/characters", "Characters")]
    [InlineData("/experience-system/brand/writing", "Writing")]
    [InlineData("/experience-system/ui", "UI Design System")]
    [InlineData("/experience-system/ui/foundations", "Foundations")]
    [InlineData("/experience-system/ui/components", "Components")]
    [InlineData("/experience-system/ui/product-patterns", "Product Patterns")]
    [InlineData("/experience-system/ui/interaction", "Interaction")]
    [InlineData("/experience-system/ui/layout", "Layout")]
    [InlineData("/experience-system/ux", "UX System")]
    [InlineData("/experience-system/ux/accessibility", "Accessibility")]
    [InlineData("/experience-system/ux/responsive", "Responsive")]
    [InlineData("/experience-system/ux/localization", "Localization")]
    [InlineData("/experience-system/ux/motion", "Motion")]
    [InlineData("/experience-system/ux/performance", "Performance")]
    public async Task EveryRouteRendersItsHeadingWithoutHorizontalOverflow(string route, string heading)
    {
        await GotoAsync(route);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = heading, Level = 1 })).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(1280, 800)]
    public async Task RootPageLinksToAllThreePillarsWithoutOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/experience-system");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "beeday Experience System", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/brand']")).ToContainTextAsync("Brand System");
        await Expect(Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/ui']")).ToContainTextAsync("UI Design System");
        await Expect(Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/ux']")).ToContainTextAsync("UX System");
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task FooterLinksToBrandGuidelinesWhichLinksOnToTheExperienceSystem()
    {
        // EPIC 27 Sprint 27.4: the footer's standalone "beeday Experience System" entry was removed
        // in favor of "Brand guidelines" (03_DESIGN_DECISIONS.md §9/§11) — the public destination
        // that, as of Sprint 27.8, will host the Experience System directly; until then it links on.
        await GotoAsync("/");

        var footerLink = Page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "Brand guidelines", Exact = true });
        await Expect(footerLink).ToHaveAttributeAsync("href", "/brand-guidelines");
        await Expect(Page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "beeday Experience System" })).ToHaveCountAsync(0);

        await footerLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/brand-guidelines$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Brand guidelines", Level = 1 })).ToBeVisibleAsync();

        var experienceSystemLink = Page.GetByRole(AriaRole.Link, new() { Name = "Open the beeday Experience System" });
        await experienceSystemLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/experience-system$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "beeday Experience System", Level = 1 })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task InternalNavigationMovesFromRootThroughPillarOverviewToTopic()
    {
        await GotoAsync("/experience-system");

        await Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/brand']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/experience-system/brand$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Brand System", Level = 1 })).ToBeVisibleAsync();

        await Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/brand/color']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/experience-system/brand/color$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Color", Level = 1 })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "UI Design System", Exact = true }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/experience-system/ui$"));
    }

    [Fact]
    public async Task PortugueseRootLocalizesHeadingAndClosingSectionWithoutOverflow()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync("/experience-system");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Português (Brasil)" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "beeday Experience System", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Construído a partir do que já está no ar")).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
