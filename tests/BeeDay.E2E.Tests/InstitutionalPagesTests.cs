using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>EPIC 27 Sprint 27.3 — the 12 institutional routes and their shared hero shell.</summary>
public sealed class InstitutionalPagesTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Theory]
    [InlineData("/mission", "Our mission")]
    [InlineData("/efficacy", "Efficacy")]
    [InlineData("/brand-guidelines", "Brand guidelines")]
    [InlineData("/contact", "Contact us")]
    [InlineData("/beeday", "beeday")]
    [InlineData("/beeday-plus", "beeday Plus")]
    [InlineData("/android", "beeday for Android")]
    [InlineData("/ios", "beeday for iOS")]
    [InlineData("/faqs", "beeday FAQs")]
    [InlineData("/community-guidelines", "Community guidelines")]
    [InlineData("/terms", "Terms of use")]
    [InlineData("/privacy", "Privacy policy")]
    public async Task EveryRouteRendersItsHeroAndHeadingWithoutHorizontalOverflowOnDesktop(string route, string heading)
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync(route);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = heading, Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.Locator(".beeday-hero")).ToBeVisibleAsync();
        await Expect(Page.Locator(".beeday-hero .beeday-brand")).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData("/mission", "Our mission")]
    [InlineData("/efficacy", "Efficacy")]
    [InlineData("/contact", "Contact us")]
    [InlineData("/beeday", "beeday")]
    [InlineData("/beeday-plus", "beeday Plus")]
    [InlineData("/android", "beeday for Android")]
    [InlineData("/ios", "beeday for iOS")]
    [InlineData("/faqs", "beeday FAQs")]
    [InlineData("/terms", "Terms of use")]
    public async Task RepresentativeRoutesRenderWithoutHorizontalOverflowOnMobile(string route, string heading)
    {
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync(route);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = heading, Level = 1 })).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData("/mission", "rgb(82, 71, 249)")]
    [InlineData("/efficacy", "rgb(82, 71, 249)")]
    [InlineData("/brand-guidelines", "rgb(82, 71, 249)")]
    [InlineData("/contact", "rgb(82, 71, 249)")]
    [InlineData("/beeday", "rgb(82, 71, 249)")]
    [InlineData("/beeday-plus", "rgb(82, 71, 249)")]
    [InlineData("/android", "rgb(82, 71, 249)")]
    [InlineData("/ios", "rgb(82, 71, 249)")]
    [InlineData("/faqs", "rgb(82, 71, 249)")]
    [InlineData("/community-guidelines", "rgb(16, 15, 62)")]
    [InlineData("/terms", "rgb(16, 15, 62)")]
    [InlineData("/privacy", "rgb(16, 15, 62)")]
    public async Task PageHeaderIsFullBleedAxisAlignedWithBodyAndUsesAPageHeaderEligibleColor(string route, string expectedBackgroundColor)
    {
        // Sprint 29.4: the header+hero is now a single surface (EditorialLayout, no separate white
        // PublicHeader) and reaches the true viewport edges via BeeDayHero's --beeday-hero-bleed-inset
        // (opted into by .editorial-layout__main in polish.css) — genuine edge-to-edge, not just
        // "wider than a centered card". Brand guidelines moved here from the ExperienceSystemHome
        // shell in this Sprint, now sharing Cor0 with the rest of its "About us" family instead of
        // the Cor8 it used before. Every route's color is one of the two COR0-COR9 tokens whose
        // contrast with white text passes WCAG AA (Cor0 #5247F9 or Cor8 #100F3E —
        // docs/brand/03-color-palette.md).
        await Page.SetViewportSizeAsync(1920, 1000);
        await GotoAsync(route);

        var hero = Page.Locator(".beeday-hero");
        var heroRow = Page.Locator(".beeday-hero__row");
        var body = Page.Locator(".institutional-page__body");
        await Expect(hero).ToBeVisibleAsync();

        Assert.Equal(expectedBackgroundColor, await hero.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));

        var heroBox = await hero.BoundingBoxAsync();
        var heroRowBox = await heroRow.BoundingBoxAsync();
        var bodyBox = await body.BoundingBoxAsync();
        Assert.NotNull(heroBox);
        Assert.NotNull(heroRowBox);
        Assert.NotNull(bodyBox);

        Assert.InRange(heroBox!.X, 0, 1);
        Assert.InRange(Math.Abs(heroBox.Width - 1920), 0, 1);
        Assert.InRange(Math.Abs(heroRowBox!.X - bodyBox!.X), 0, 1);
        Assert.InRange(Math.Abs(heroRowBox.Width - bodyBox.Width), 0, 1);
    }

    [Fact]
    public async Task FaqsAccordionIsKeyboardOperableAndTogglesWithoutHover()
    {
        await GotoAsync("/faqs");

        var firstQuestion = Page.GetByText("What is beeday?", new() { Exact = true });
        await Expect(firstQuestion).ToBeVisibleAsync();
        await firstQuestion.FocusAsync();
        Assert.True(await firstQuestion.EvaluateAsync<bool>("element => element.matches(':focus-visible')"));

        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.GetByText("beeday is a personal productivity application")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task FaqsHelpAnswerLinksThroughToTheRealContactPage()
    {
        await GotoAsync("/faqs");

        await Page.GetByText("Where can I get help or report an issue?", new() { Exact = true }).ClickAsync();
        var contactLink = Page.Locator(".institutional-faq__item").GetByRole(AriaRole.Link, new() { Name = "Contact us", Exact = true });
        await Expect(contactLink).ToBeVisibleAsync();
        await Expect(contactLink).ToHaveAttributeAsync("href", "/contact");
        Assert.Equal("none", await contactLink.EvaluateAsync<string>("element => getComputedStyle(element).textDecorationLine"));

        await contactLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/contact$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Contact us", Level = 1 })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task FaqsAccordionTogglesOnTouchTapWithoutHover()
    {
        await using var touchContext = await Fixture.Browser.NewContextAsync(new() { HasTouch = true, ViewportSize = new() { Width = 390, Height = 844 } });
        var touchPage = await touchContext.NewPageAsync();
        await touchPage.GotoAsync($"{Fixture.ServerAddress}/faqs");
        await touchPage.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var question = touchPage.GetByText("Is beeday available on mobile?", new() { Exact = true });
        await question.TapAsync();
        await Expect(touchPage.GetByText("dedicated Android and iOS apps are coming soon")).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData("/beeday", "Get started")]
    [InlineData("/beeday-plus", "Try beeday today")]
    [InlineData("/android", "Try beeday today")]
    [InlineData("/ios", "Try beeday today")]
    public async Task ProductAndAppPagesKeepTheHeroCtaFullyVisibleOnMobileAndItLinksToRegistration(string route, string ctaLabel)
    {
        // EPIC 27 Sprint 27.6 acceptance: "Mobile não corta hero/CTA."
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync(route);

        var cta = Page.GetByRole(AriaRole.Link, new() { Name = ctaLabel, Exact = true });
        await Expect(cta).ToBeVisibleAsync();
        await Expect(cta).ToHaveAttributeAsync("href", "/profile/create");
        var box = await cta.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.X, 0, 390);
        Assert.True(box.X + box.Width <= 390 + 1, "CTA extends past the mobile viewport width.");
    }

    [Fact]
    public async Task EfficacyDisclosesNoPublishedEvidenceInsteadOfFabricatedMetrics()
    {
        await GotoAsync("/efficacy");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "What we'll publish here", Level = 2 })).ToBeVisibleAsync();
        await Expect(Page.GetByText("not yet published", new() { Exact = false })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ContactUsLinksToTheRealExistingGitHubAndLinkedInChannels()
    {
        await GotoAsync("/contact");

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Open an issue on GitHub" }))
            .ToHaveAttributeAsync("href", "https://github.com/tiagoarrigoni/BeeDay");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Connect on LinkedIn" }))
            .ToHaveAttributeAsync("href", "https://www.linkedin.com/in/tiago-a-arrigoni-335b9413b/");
    }

    [Fact]
    public async Task LegalPagesDisclosePendingReviewInsteadOfFabricatedClauses()
    {
        await GotoAsync("/terms");

        await Expect(Page.GetByText("pending review", new() { Exact = false })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Sections", Level = 2 })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NoDuolingoReferenceExistsOnAnyInstitutionalRoute()
    {
        foreach (var route in new[] { "/mission", "/efficacy", "/brand-guidelines", "/contact", "/beeday", "/beeday-plus", "/android", "/ios", "/faqs", "/community-guidelines", "/terms", "/privacy" })
        {
            await GotoAsync(route);
            Assert.DoesNotContain("duolingo", Page.Url, StringComparison.OrdinalIgnoreCase);
            var html = await Page.ContentAsync();
            Assert.DoesNotContain("duolingo", html, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task NoWhitePublicHeaderFlagsOrContinueButtonRenderOnAnyEditorialRoute()
    {
        foreach (var route in new[] { "/mission", "/efficacy", "/brand-guidelines", "/contact", "/beeday", "/beeday-plus", "/android", "/ios", "/faqs", "/community-guidelines", "/terms", "/privacy" })
        {
            await GotoAsync(route);
            Assert.Equal(0, await Page.Locator(".public-header").CountAsync());
            Assert.Equal(0, await Page.Locator(".public-language-switcher").CountAsync());
            await Expect(Page.GetByText("Continue to beeday", new() { Exact = true })).Not.ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ContextualNavigationOnMissionShowsOnlyItsOwnAboutUsFamilyWithCurrentPageMarked()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/mission");

        var nav = Page.Locator(".editorial-section-nav");
        await Expect(nav).ToBeVisibleAsync();
        await Expect(nav.GetByRole(AriaRole.Link)).ToHaveCountAsync(4);
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "Mission", Exact = true })).ToHaveAttributeAsync("aria-current", "page");
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "Efficacy", Exact = true })).ToBeVisibleAsync();
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "beeday for Android", Exact = true })).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task EditorialFooterBackToTopScrollsToTheTopAndBuyMeACoffeeLinksToItsContractualRoute()
    {
        // A short viewport guarantees the page overflows vertically regardless of how tall Mission's
        // own content happens to be, so Back to Top always has real scroll distance to undo.
        await Page.SetViewportSizeAsync(1280, 500);
        await GotoAsync("/mission");
        await Page.Locator(".editorial-footer").ScrollIntoViewIfNeededAsync();
        var scrollYAfterScrollingToFooter = await Page.EvaluateAsync<int>("() => window.scrollY");
        Assert.True(scrollYAfterScrollingToFooter > 0, $"expected scrollY > 0 after scrolling to the footer, got {scrollYAfterScrollingToFooter}.");

        var backToTop = Page.GetByRole(AriaRole.Button, new() { Name = "Back to top" });
        await Expect(backToTop).ToBeVisibleAsync();
        await backToTop.ClickAsync();
        await Page.WaitForFunctionAsync("() => window.scrollY === 0");

        var coffee = Page.GetByRole(AriaRole.Link, new() { Name = "BUY ME A COFFEE", Exact = true });
        await Expect(coffee).ToBeVisibleAsync();
        await Expect(coffee).ToHaveAttributeAsync("href", "/buy-me-a-coffee");

        // Sprint 29.4 §27: Back to Top sits at the row start, independent of the centered coffee link.
        var backToTopBox = await backToTop.BoundingBoxAsync();
        var coffeeBox = await coffee.BoundingBoxAsync();
        Assert.NotNull(backToTopBox);
        Assert.NotNull(coffeeBox);
        Assert.True(backToTopBox!.X < coffeeBox!.X, "Back to Top must sit to the left of the centered Buy Me a Coffee link.");
    }

    [Fact]
    public async Task MobileEditorialHeaderStaysUsableWithoutHorizontalOverflowOnTheDenseAboutUsFamily()
    {
        // "About us" is the densest family (4 links) — the representative case for §11's mobile
        // navigation requirement.
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync("/mission");

        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        await Expect(Page.Locator(".editorial-section-nav")).ToBeVisibleAsync();
    }
}
