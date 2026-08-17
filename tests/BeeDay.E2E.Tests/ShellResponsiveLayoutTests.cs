using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class ShellResponsiveLayoutTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Theory]
    [InlineData(1920, 900)]
    [InlineData(1440, 900)]
    [InlineData(1280, 800)]
    [InlineData(1200, 800)]
    public async Task DesktopUsesNeutralNavigationAndWideWorkspaceWithoutLegacyRegions(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await LoginToDailyAsync();

        var sidebar = Page.Locator(".desktop-sidebar");
        await Expect(sidebar).ToBeVisibleAsync();
        var box = await sidebar.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.Width, 244, 252);
        Assert.Equal("rgb(255, 255, 255)", await sidebar.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        var dailyItem = sidebar.Locator("a[href='/daily']");
        Assert.Equal("flex", await dailyItem.EvaluateAsync<string>("element => getComputedStyle(element).display"));
        var iconBox = await dailyItem.Locator(".navigation-item__icon").BoundingBoxAsync();
        var labelBox = await dailyItem.Locator(".navigation-item__label").BoundingBoxAsync();
        Assert.NotNull(iconBox);
        Assert.NotNull(labelBox);
        Assert.InRange(Math.Abs((iconBox!.Y + iconBox.Height / 2) - (labelBox!.Y + labelBox.Height / 2)), 0, 2);
        await Expect(Page.Locator(".mobile-header")).ToBeHiddenAsync();
        await AssertRetiredRegionsAbsentAsync();

        var columns = Page.Locator(".dashboard-grid > *");
        await Expect(columns).ToHaveCountAsync(4);
        foreach (var column in await columns.AllAsync())
        {
            var columnBox = await column.BoundingBoxAsync();
            Assert.NotNull(columnBox);
            Assert.True(columnBox!.Width >= 240, $"Daily column was only {columnBox.Width}px at {width}px.");
        }
        Assert.False(await HasDocumentOverflowAsync());
    }

    [Fact]
    public async Task DesktopSidebarCentersItsBrandAndGivesAccountATextSafeAccentWhenNotActive()
    {
        // EPIC 27 Sprint 27.9 acceptance: "Marca está realmente centralizada na largura útil da
        // sidebar" / "Account usa accent sem parecer estado ativo de navegação quando não for."
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        var brandLink = Page.Locator("a.desktop-sidebar__brand-link");
        var brandContent = brandLink.Locator(".beeday-brand");
        var linkBox = await brandLink.BoundingBoxAsync();
        var contentBox = await brandContent.BoundingBoxAsync();
        Assert.NotNull(linkBox);
        Assert.NotNull(contentBox);
        var linkCenter = linkBox!.X + linkBox.Width / 2;
        var contentCenter = contentBox!.X + contentBox.Width / 2;
        Assert.InRange(Math.Abs(linkCenter - contentCenter), 0, 2);

        // /daily is the current route here, so Account (/settings) must show only its resting
        // accent — never the active-route pill it isn't entitled to.
        var accountItem = Page.Locator(".desktop-sidebar a[href='/settings']");
        Assert.Equal("rgb(11, 114, 166)", await accountItem.EvaluateAsync<string>("element => getComputedStyle(element).color"));
        Assert.Null(await accountItem.GetAttributeAsync("aria-current"));
        Assert.Equal("rgba(0, 0, 0, 0)", await accountItem.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }

    [Theory]
    [InlineData(1024, 800)]
    [InlineData(1199, 800)]
    [InlineData(900, 800)]
    [InlineData(768, 900)]
    [InlineData(430, 900)]
    [InlineData(390, 844)]
    public async Task TabletAndMobileUseOneDrawerShellWithoutDocumentOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await LoginToDailyAsync();
        await Expect(Page.Locator(".desktop-sidebar")).ToBeHiddenAsync();
        await Expect(Page.Locator(".mobile-header")).ToBeVisibleAsync();
        await AssertRetiredRegionsAbsentAsync();
        Assert.False(await HasDocumentOverflowAsync());

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        var drawer = Page.Locator("#mobile-navigation");
        await Expect(drawer).ToBeVisibleAsync();
        await Expect(drawer.Locator("a[href='/settings']")).ToBeVisibleAsync();
        await Expect(drawer.GetByRole(AriaRole.Button, new() { Name = "Log out of beeday" })).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(drawer).ToBeHiddenAsync();
    }

    [Fact]
    public async Task MobileHeaderAndDrawerBrandLinksRenderWithoutTheBrowserDefaultUnderline()
    {
        // EPIC 27 Sprint 27.12 (epic hardening): regression lock for the missing-::deep bug found
        // in MobileHeader/MobileSidebar's brand links (same class of bug as DesktopSidebar,
        // Sprint 27.9) — both used to render the wordmark with the browser's default underline.
        await Page.SetViewportSizeAsync(390, 844);
        await LoginToDailyAsync();

        var headerBrand = Page.Locator(".mobile-header__brand");
        Assert.Equal("none", await headerBrand.EvaluateAsync<string>("element => getComputedStyle(element).textDecorationLine"));
        Assert.Equal("flex", await headerBrand.EvaluateAsync<string>("element => getComputedStyle(element).display"));

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        var drawerBrand = Page.Locator("#mobile-navigation .mobile-nav-drawer__brand");
        await Expect(drawerBrand).ToBeVisibleAsync();
        Assert.Equal("none", await drawerBrand.EvaluateAsync<string>("element => getComputedStyle(element).textDecorationLine"));
        Assert.Equal("flex", await drawerBrand.EvaluateAsync<string>("element => getComputedStyle(element).display"));
    }

    private async Task AssertRetiredRegionsAbsentAsync()
    {
        await Expect(Page.Locator(".right-rail, .side-drawer, .support-drawer, .app-footer")).ToHaveCountAsync(0);
    }

    private async Task<bool> HasDocumentOverflowAsync() =>
        await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-shell-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");
    }
}
