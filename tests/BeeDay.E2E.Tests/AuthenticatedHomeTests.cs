using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>Epic 21 Sprint 21.10 authenticated Home composition in real Chromium.</summary>
public sealed class AuthenticatedHomeTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task LoginEntersActionFirstHomeWithDesktopShellAndValidEmptyState()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("Welcome back") })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Your next step is yours to choose" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".desktop-sidebar nav.navigation-items a[href='/home']")).ToHaveAttributeAsync("aria-current", "page");
        await Expect(Page.Locator(".right-rail")).ToBeVisibleAsync();
        await Expect(Page.Locator(".product-home__mobile-progress")).ToBeHiddenAsync();
        await AssertNoOverflowAsync();

        await Page.Locator(".desktop-sidebar a[href='/daily']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/daily$"));
        await Page.Locator(".desktop-sidebar nav.navigation-items a[href='/home']").ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));
    }

    [Fact]
    public async Task MobileAndTabletPrioritizeEssentialProgressWithoutRightRailOrOverflow()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await LoginAsync();

        await Expect(Page.Locator(".right-rail")).ToBeHiddenAsync();
        await Expect(Page.Locator(".product-home__mobile-progress")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Progressbar, new() { Name = "Experience progress" })).ToBeVisibleAsync();
        await AssertNoOverflowAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await Expect(Page.Locator("#mobile-navigation nav.navigation-items a[href='/home']")).ToHaveAttributeAsync("aria-current", "page");

        await Page.SetViewportSizeAsync(900, 800);
        await Expect(Page.Locator(".right-rail")).ToBeHiddenAsync();
        await AssertNoOverflowAsync();
    }

    [Fact]
    public async Task RealTaskAppearsAndCanBeCompletedFromHome()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginAsync();
        await GotoAsync("/daily");
        var title = $"Home task {Guid.NewGuid():N}"[..22];

        await Page.GetByRole(AriaRole.Button, new() { Name = "Activity" }).ClickAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await GotoAsync("/home");
        var complete = Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" });
        await Expect(complete).ToBeVisibleAsync();
        await complete.ClickAsync();
        await Expect(complete).ToBeHiddenAsync();
    }

    private async Task LoginAsync()
    {
        var email = $"e2e-home-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task AssertNoOverflowAsync() =>
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
}
