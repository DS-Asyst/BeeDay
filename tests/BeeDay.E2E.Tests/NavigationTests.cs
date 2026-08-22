using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Sprint 21.3 (EPIC 21) — BeeDay Navigation, verified against a real Chromium render: active-route
/// indication, deep links, and the mobile drawer's open/close/Escape/keyboard/focus behavior (none
/// of which bUnit can verify, since it has no layout engine and no real keyboard focus). See
/// docs/epics/21-lingo-product-experience/README.md "Sprint 21.3".
/// </summary>
public sealed class NavigationTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task Desktop_ClickingWalletNavigatesAndMarksItAsCurrentInsteadOfDaily()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        var dailyLink = Page.Locator(".desktop-sidebar nav.navigation-items a[href='/daily']");
        var walletLink = Page.Locator(".desktop-sidebar nav.navigation-items a[href='/wallet']");
        await Expect(dailyLink).ToHaveAttributeAsync("aria-current", "page");
        await Expect(walletLink).Not.ToHaveAttributeAsync("aria-current", "page");

        await walletLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(walletLink).ToHaveAttributeAsync("aria-current", "page");
        await Expect(dailyLink).Not.ToHaveAttributeAsync("aria-current", "page");
    }

    [Fact]
    public async Task Desktop_DeepLinkToWalletRendersWithSidebarAndWalletMarkedActive()
    {
        await Page.SetViewportSizeAsync(1280, 800);

        var email = $"e2e-nav-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await LoginAsync(email);

        await GotoAsync("/wallet");

        await Expect(Page.Locator(".desktop-sidebar")).ToBeVisibleAsync();
        await Expect(Page.Locator(".desktop-sidebar nav.navigation-items a[href='/wallet']")).ToHaveAttributeAsync("aria-current", "page");
    }

    [Fact]
    public async Task Mobile_HamburgerOpensDrawer_EscapeCloses()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        var drawer = Page.Locator("#mobile-navigation");
        await Expect(drawer).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await Expect(drawer).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Close navigation menu" })).ToBeVisibleAsync(); // the header's own toggle, now flipped

        // Keyboard focus actually moves into the drawer on open (not just visually shown).
        await Expect(drawer.GetByRole(AriaRole.Button, new() { Name = "Close", Exact = true })).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(drawer).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Mobile_ClickingBackdropCloses()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await Expect(Page.Locator("#mobile-navigation")).ToBeVisibleAsync();

        // Click far from the drawer's own left-hand bounds (min(85vw, 17rem) wide) so the click
        // actually lands on the backdrop instead of being intercepted by the drawer itself.
        await Page.Locator(".mobile-nav-backdrop").ClickAsync(new() { Position = new Position { X = 370, Y = 400 } });
        await Expect(Page.Locator("#mobile-navigation")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Mobile_NavigatingToWalletFromTheDrawerClosesItAndReachesWallet()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await Page.Locator("#mobile-navigation a[href='/wallet']").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Expect(Page.Locator("#mobile-navigation")).ToBeHiddenAsync();
    }

    // EPIC 30 Sprint 30.20: the drawer already moves focus to its own close button on open, but
    // nothing previously returned focus to the hamburger trigger when it closed — via Escape, the
    // backdrop, or the close button — leaving keyboard/screen-reader focus lost to <body>.
    [Fact]
    public async Task Mobile_ClosingTheDrawerReturnsFocusToTheHamburgerTrigger()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        var trigger = Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" });
        await trigger.ClickAsync();
        await Expect(Page.Locator("#mobile-navigation")).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(Page.Locator("#mobile-navigation")).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" })).ToBeFocusedAsync();
    }

    [Fact]
    public async Task Mobile_CloseButtonCloses()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        var drawer = Page.Locator("#mobile-navigation");
        await Expect(drawer).ToBeVisibleAsync();

        // The drawer's own dedicated close control (distinct from the header's toggle button).
        await drawer.GetByRole(AriaRole.Button, new() { Name = "Close", Exact = true }).ClickAsync();
        await Expect(drawer).ToBeHiddenAsync();
    }

    // EPIC 30 Sprint 30.17: NotFoundTests.cs (bUnit) already renders the NotFound component
    // directly, bypassing the router entirely. Nothing previously proved a genuinely nonexistent
    // URL actually reaches Routes.razor's NotFoundPage fallback in a real browser.
    [Fact]
    public async Task NonexistentRoute_RendersTheNotFoundPage()
    {
        await GotoAsync("/this-route-does-not-exist-e2e");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Not Found" })).ToBeVisibleAsync();
    }

    // Sprint 32.2 (EPIC 32, EXP32-F014): a genuinely nonexistent route used to reach NotFound via
    // UseStatusCodePagesWithReExecute, which kept the broken URL in the address bar. Once the
    // interactive circuit booted, Blazor's own client-side Router independently re-resolved that
    // same unmatched URL and rendered its NotFoundPage fallback a second time with no HttpContext
    // to source a fresh antiforgery token from — silently clobbering the correctly-prerendered
    // Logout form with one missing its token, so Logout failed with a raw 400 ProblemDetails page
    // for any authenticated user who landed on a broken link. Redirecting to the real /not-found
    // route (Program.cs now redirects 404s specifically instead of re-executing) puts the same,
    // matching URL in front of both the server render and the client Router, so no fallback
    // re-render happens.
    [Fact]
    public async Task NonexistentRoute_WhileAuthenticated_LogoutStillWorks()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        await GotoAsync("/route-that-genuinely-does-not-exist-e2e");
        await Expect(Page).ToHaveURLAsync(new Regex("/not-found$"));

        await Page.GetByRole(AriaRole.Button, new() { Name = "Log out of beeday" }).First.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/login"));
    }

    // Sprint 32.2 (EPIC 32, EXP32-F015): NavigationItem only ever matched its own Href, so the
    // sidebar's Account entry (Href="/settings") never showed as current for /account — the same
    // page's pre-existing compatibility alias (docs/web/02-routing-and-pages.md §3).
    [Fact]
    public async Task Desktop_VisitingTheAccountAliasStillMarksAccountAsCurrent()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        await GotoAsync("/account");

        await Expect(Page.Locator(".desktop-sidebar a[href='/settings']")).ToHaveAttributeAsync("aria-current", "page");
    }

    // Sprint 32.2 (EPIC 32): nothing previously proved the browser's own Back/Forward buttons
    // actually work against Blazor Server's client-side navigation (as opposed to a full page
    // reload) — active-route state must track the real current URL, not just the URL clicked last.
    [Fact]
    public async Task Desktop_BrowserBackAndForwardNavigateAndKeepActiveStateInSync()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        var dailyLink = Page.Locator(".desktop-sidebar nav.navigation-items a[href='/daily']");
        var walletLink = Page.Locator(".desktop-sidebar nav.navigation-items a[href='/wallet']");

        await walletLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GoBackAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/daily$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(dailyLink).ToHaveAttributeAsync("aria-current", "page");
        await Expect(walletLink).Not.ToHaveAttributeAsync("aria-current", "page");

        await Page.GoForwardAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(walletLink).ToHaveAttributeAsync("aria-current", "page");
        await Expect(dailyLink).Not.ToHaveAttributeAsync("aria-current", "page");
    }

    [Fact]
    public async Task Desktop_LogoutRemainsDirectlyAccessibleInSecondaryNavigation()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Log out of beeday" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".desktop-sidebar nav.navigation-items a[href='/profile']")).ToBeVisibleAsync();
        await Expect(Page.Locator(".desktop-sidebar a[href='/settings']")).ToBeVisibleAsync();
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-nav-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await LoginAsync(email);
    }

    private async Task LoginAsync(string email)
    {
        await SubmitLoginAsync(email, Password);
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
