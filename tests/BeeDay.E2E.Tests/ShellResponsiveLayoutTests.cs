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

    [Theory]
    [InlineData(1024, 800)]
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
