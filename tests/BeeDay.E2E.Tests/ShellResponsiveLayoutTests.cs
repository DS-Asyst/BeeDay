using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Sprint 21.2 (EPIC 21) shell foundation, verified against a real Chromium render — bUnit has no
/// layout engine, so the actual geometry/visibility contract (which region is on screen, at which
/// width, with no horizontal overflow) can only be confirmed here. See
/// docs/epics/21-lingo-product-experience/README.md §3/§13/§22.
/// </summary>
public sealed class ShellResponsiveLayoutTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task DesktopViewport_ShowsPersistentSidebarAndRightRail_HidesTopNavigation_NoHorizontalOverflow()
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

        await Expect(Page.Locator(".top-navigation")).ToBeHiddenAsync();

        Assert.False(await HasHorizontalOverflowAsync());
    }

    [Fact]
    public async Task NarrowViewport_HidesSidebarAndRightRail_PreservesTopNavigationAccessToProfilePanel()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        await Expect(Page.Locator(".desktop-sidebar")).ToBeHiddenAsync();
        await Expect(Page.Locator(".right-rail")).ToBeHiddenAsync();
        await Expect(Page.Locator(".top-navigation")).ToBeVisibleAsync();

        Assert.False(await HasHorizontalOverflowAsync());

        // The transitory mobile fallback (§8 of the Sprint) must still give real access to the
        // profile panel, exactly as before this Sprint's shell change.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Open profile panel" }).ClickAsync();
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
        await Expect(Page).ToHaveURLAsync(new Regex("/daily$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
