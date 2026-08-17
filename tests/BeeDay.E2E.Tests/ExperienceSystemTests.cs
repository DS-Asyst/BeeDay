using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class ExperienceSystemTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Theory]
    [InlineData("/experience-system", "Brand guidelines")]
    [InlineData("/brand-guidelines", "Brand guidelines")]
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

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Brand guidelines", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/brand']")).ToContainTextAsync("Brand System");
        await Expect(Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/ui']")).ToContainTextAsync("UI Design System");
        await Expect(Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/ux']")).ToContainTextAsync("UX System");
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task FooterLinksDirectlyToTheBrandGuidelinesHeroAndDocumentation()
    {
        // EPIC 27 Sprint 27.4: the footer's standalone "beeday Experience System" entry was removed
        // in favor of "Brand guidelines" (03_DESIGN_DECISIONS.md §9/§11). Sprint 27.8 made
        // "/brand-guidelines" a second route directly on the same page that already serves
        // "/experience-system", so the footer now lands the visitor on the real content immediately
        // — no separate landing-then-link-onward hop.
        await GotoAsync("/");

        var footerLink = Page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "Brand guidelines", Exact = true });
        await Expect(footerLink).ToHaveAttributeAsync("href", "/brand-guidelines");
        await Expect(Page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "beeday Experience System" })).ToHaveCountAsync(0);

        await footerLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/brand-guidelines$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Brand guidelines", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.Locator("header.beeday-hero")).ToBeVisibleAsync();

        await Page.Locator("a.experience-system-topic-grid__card[href='/experience-system/brand']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/experience-system/brand$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Brand System", Level = 1 })).ToBeVisibleAsync();
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
    public async Task DesktopSidebarIsStickyWithTheActiveTopicEvident()
    {
        // EPIC 27 Sprint 27.8 acceptance: "Menu lateral ... hierarquia clara" / "item ativo evidente."
        await Page.SetViewportSizeAsync(1280, 900);
        await GotoAsync("/experience-system/ui/foundations");

        var sidebar = Page.Locator(".experience-system-sidebar");
        await Expect(sidebar).ToBeVisibleAsync();
        Assert.Equal("sticky", await sidebar.EvaluateAsync<string>("element => getComputedStyle(element).position"));

        // Both the active pillar ("UI Design System") and the active topic ("Foundations") carry
        // aria-current="page" at once — NavLink's default prefix match on the pillar nav is
        // pre-existing, unrelated behavior (untouched by this sprint); scope to the topic nav so
        // this asserts the topic-level "item ativo evidente" acceptance criterion specifically.
        var activeTopic = Page.Locator(".experience-system-topic-nav a[aria-current='page']");
        await Expect(activeTopic).ToHaveTextAsync("Foundations");

        // The toggle only exists for the mobile/tablet collapse — it must not compete for attention
        // once the sidebar is already always visible and sticky.
        await Expect(Page.Locator(".experience-system-sidebar__toggle")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task MobileSidebarCollapsesAndNeverBecomesAFixedOverlay()
    {
        // EPIC 27 Sprint 27.8 acceptance: "Mobile não mantém sidebar fixa ocupando viewport."
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync("/experience-system/ui/foundations");

        var sidebar = Page.Locator(".experience-system-sidebar");
        var toggle = Page.Locator(".experience-system-sidebar__toggle");
        await Expect(toggle).ToBeVisibleAsync();

        var position = await sidebar.EvaluateAsync<string>("element => getComputedStyle(element).position");
        Assert.NotEqual("fixed", position);
        Assert.NotEqual("sticky", position);

        var activeTopic = Page.Locator(".experience-system-topic-nav a[aria-current='page']");
        await Expect(activeTopic).ToBeVisibleAsync();
        await toggle.ClickAsync();
        await Expect(Page.Locator(".experience-system-sidebar__content")).ToBeHiddenAsync();

        await toggle.ClickAsync();
        await Expect(activeTopic).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task PortugueseRootLocalizesHeadingAndClosingSectionWithoutOverflow()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync("/experience-system");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Português (Brasil)" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Brand guidelines", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Construído a partir do que já está no ar")).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
