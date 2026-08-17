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
    [InlineData("/beeday", "beeday")]
    [InlineData("/faqs", "beeday FAQs")]
    [InlineData("/terms", "Terms of use")]
    public async Task RepresentativeRoutesRenderWithoutHorizontalOverflowOnMobile(string route, string heading)
    {
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync(route);

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = heading, Level = 1 })).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
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
