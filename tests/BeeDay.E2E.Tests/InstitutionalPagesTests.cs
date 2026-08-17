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
        // Sprint 29.2: the hero used to render as a small card capped to 72rem by an accidental CSS
        // cascade interaction (nesting it inside a reading-width-limited <article>) — this protects
        // three things at once: the header spans the full viewport width (not just 72rem), its
        // content row shares the same left edge as the body content below it, and every route's
        // color is one of the two COR0-COR9 tokens whose contrast with white text passes WCAG AA
        // (Cor0 #5247F9 or Cor8 #100F3E — docs/brand/03-color-palette.md).
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

        // The hero fills essentially the whole 1920px viewport (only .beeday-main's own <= 2rem
        // gutter stands between it and the edge) instead of being capped to the 72rem reading width
        // a nested <article> used to impose on it — same full-bleed behavior ExperienceSystemHome's
        // already-correct sibling hero has. Before this Sprint's fix, a centered 72rem card here
        // would sit at X ~ (1920 - 1152) / 2 = 384px and be only 1152px wide.
        Assert.True(heroBox!.X < 40, $"the hero should start near the viewport edge, not be centered as a card (X={heroBox.X}).");
        Assert.True(heroBox.Width > 1800, $"the hero must not be capped to the 72rem reading width (Width={heroBox.Width}).");
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
}
