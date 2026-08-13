using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// EPIC 21 shell foundation (Sprint 21.2) and its real navigation (Sprint 21.3), verified against a
/// real Chromium render — bUnit has no layout engine, so the actual geometry/visibility contract
/// (which region is on screen, at which width, with no horizontal overflow) can only be confirmed
/// here. Mobile navigation open/close/keyboard behavior is covered separately in
/// <see cref="NavigationTests"/>. See docs/epics/21-lingo-product-experience/README.md §3/§13/§22
/// and "Sprint 21.3".
/// </summary>
public sealed class ShellResponsiveLayoutTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task DesktopViewport_ShowsPersistentSidebarAndRightRail_HidesMobileHeader_NoHorizontalOverflow()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        var sidebar = Page.Locator(".desktop-sidebar");
        await Expect(sidebar).ToBeVisibleAsync();
        var sidebarBox = await sidebar.BoundingBoxAsync();
        Assert.NotNull(sidebarBox);
        Assert.InRange(sidebarBox!.Width, 250, 262); // 16rem = 256px

        var rightRail = Page.Locator(".right-rail");
        await Expect(rightRail).ToBeVisibleAsync();
        var rightRailBox = await rightRail.BoundingBoxAsync();
        Assert.NotNull(rightRailBox);
        Assert.InRange(rightRailBox!.Width, 362, 374); // 23rem = 368px
        Assert.Equal("sticky", await rightRail.EvaluateAsync<string>("element => getComputedStyle(element).position"));

        await Expect(rightRail.GetByText("Level", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(rightRail.GetByText(new Regex(@"\d+ XP total"))).ToBeVisibleAsync();
        var experienceProgress = rightRail.GetByRole(AriaRole.Progressbar, new() { Name = "Experience progress" });
        await Expect(experienceProgress).ToBeVisibleAsync();
        Assert.NotNull(await experienceProgress.GetAttributeAsync("aria-valuenow"));
        Assert.NotNull(await experienceProgress.GetAttributeAsync("aria-valuemax"));
        var experienceCard = rightRail.Locator(".experience-card");
        Assert.Equal("2px", await experienceCard.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.Equal("12px", await experienceCard.EvaluateAsync<string>("element => getComputedStyle(element).borderTopLeftRadius"));
        Assert.Equal("none", await experienceCard.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
        Assert.Null(await experienceCard.GetAttributeAsync("role"));
        Assert.Null(await experienceCard.GetAttributeAsync("tabindex"));

        await Expect(Page.Locator(".mobile-header")).ToBeHiddenAsync();

        Assert.False(await HasHorizontalOverflowAsync());
    }

    [Fact]
    public async Task NarrowViewport_HidesSidebarAndRightRail_ShowsMobileHeader_ProfilePanelReachableThroughDrawer()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        await Expect(Page.Locator(".desktop-sidebar")).ToBeHiddenAsync();
        await Expect(Page.Locator(".right-rail")).ToBeHiddenAsync();
        await Expect(Page.Locator(".mobile-header")).ToBeVisibleAsync();

        Assert.False(await HasHorizontalOverflowAsync());

        // Sprint 21.3: Profile is no longer a direct button on the mobile header itself — it now
        // lives inside the hamburger drawer, alongside Daily/Wallet/Account, same as desktop.
        // Opening it closes the drawer (avoids the drawer and the profile panel, both left-anchored
        // overlays of similar width, stacking on top of each other) — the panel itself becomes
        // visible, and its own trigger (now reading "Close profile panel") is reachable again by
        // reopening the hamburger menu, verified below.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Open profile panel" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Complementary, new() { Name = "Profile panel" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Close profile panel" })).ToBeVisibleAsync();
    }

    private async Task<bool> HasHorizontalOverflowAsync() =>
        await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-shell-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));
        await GotoAsync("/daily");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
