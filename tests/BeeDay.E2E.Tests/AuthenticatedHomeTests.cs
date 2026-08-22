using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>Epic 21 Sprint 21.12 authenticated Profile composition in real Chromium.</summary>
public sealed class AuthenticatedHomeTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task LoginEntersProfileWithDesktopShellAndHonestWeeklyState()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("Welcome back") })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Weekly history is not available yet" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".desktop-sidebar nav.navigation-items a[href='/profile']")).ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.Locator(".right-rail")).ToHaveCountAsync(0);
        await Expect(Page.Locator(".product-home__progress")).ToBeVisibleAsync();
        await AssertNoOverflowAsync();

        await Page.Locator(".desktop-sidebar a[href='/daily']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/daily$"));
        await Page.Locator(".desktop-sidebar nav.navigation-items a[href='/profile']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(430, 860)]
    [InlineData(768, 900)]
    [InlineData(1024, 800)]
    [InlineData(1280, 800)]
    [InlineData(1440, 900)]
    [InlineData(1920, 1080)]
    public async Task MobileAndTabletPrioritizeEssentialProgressWithoutLegacyRegionsOrOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await LoginAsync();

        await Expect(Page.Locator(".right-rail")).ToHaveCountAsync(0);
        await Expect(Page.Locator(".product-home__progress")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Progressbar, new() { Name = "Experience progress" })).ToBeVisibleAsync();
        await AssertNoOverflowAsync();

        if (width < 768)
        {
            await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
            await Expect(Page.Locator("#mobile-navigation nav.navigation-items a[href='/profile']")).ToHaveAttributeAsync("aria-current", "page");
        }
    }

    [Fact]
    public async Task OnlyOneOpenDailyControlRemainsOnProfile()
    {
        // EPIC 27 Sprint 27.9: the redundant "Open Daily" link next to "Weekly activity" was
        // removed; the header CTA is the sole surviving control.
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Open Daily" })).ToHaveCountAsync(1);
        await Expect(Page.Locator(".product-home__section-heading a")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task LegacyHomeRouteRedirectsToProfile()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginAsync();
        await GotoAsync("/home");

        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("Welcome back") })).ToBeVisibleAsync();
    }

    private async Task LoginAsync()
    {
        var email = $"e2e-home-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await SubmitLoginAsync(email, Password);
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task AssertNoOverflowAsync() =>
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
}
