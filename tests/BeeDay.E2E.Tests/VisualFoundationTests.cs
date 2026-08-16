using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>Epic 21 Sprint 21.4 foundations verified in a real Chromium rendering engine.</summary>
public sealed class VisualFoundationTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task PublicHomeAndLogin_RenderWithNunitoSolidSurfacesAndNoOverflow()
    {
        await Page.SetViewportSizeAsync(1280, 800);

        await GotoAsync("/");
        await AssertGlobalFoundationAsync();
        await Expect(Page.Locator(".public-header__brand-mark")).ToBeVisibleAsync();

        await GotoAsync("/login");
        await AssertGlobalFoundationAsync();
        await AssertWordmarkAsync(Page.Locator(".auth-login .beeday-brand").First);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Close login and return to Home" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Create account" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".auth-card")).ToHaveCountAsync(0);
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task AuthenticatedDesktopAndMobileSurfacesRetainLayoutFocusAndTypography()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();
        await AssertGlobalFoundationAsync();
        await Expect(Page.Locator(".desktop-sidebar")).ToBeVisibleAsync();
        var desktopBrand = Page.Locator(".desktop-sidebar .beeday-brand");
        await AssertWordmarkAsync(desktopBrand);
        Assert.Equal("rgba(0, 0, 0, 0)", await desktopBrand.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));

        await GotoAsync("/profile");
        var experienceProgress = Page.Locator(".product-home__progress .experience-card .beeday-progress");
        await Expect(experienceProgress).ToHaveAttributeAsync("data-tone", "reward");
        Assert.Equal("rgb(255, 211, 38)", await experienceProgress.Locator(".beeday-progress__fill")
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));

        Assert.Equal("#5247f9", await Page.Locator("html").EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--beeday-color-brand-primary').trim()"));

        var wallet = Page.GetByRole(AriaRole.Link, new() { Name = "Wallet" });
        await wallet.FocusAsync();
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Page.Keyboard.PressAsync("Tab");
        Assert.True(await wallet.EvaluateAsync<bool>("element => element.matches(':focus-visible')"));
        Assert.NotEqual("none", await wallet.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
        await wallet.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await AssertGlobalFoundationAsync();

        await Expect(Page.Locator(".desktop-sidebar a[href='/settings']")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Log out of beeday" })).ToBeVisibleAsync();

        await Page.SetViewportSizeAsync(390, 844);
        await Expect(Page.Locator(".mobile-header")).ToBeVisibleAsync();
        await Expect(Page.Locator(".desktop-sidebar")).ToBeHiddenAsync();
        await AssertWordmarkAsync(Page.Locator(".mobile-header .beeday-brand"));
        await AssertGlobalFoundationAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" }).ClickAsync();
        await AssertWordmarkAsync(Page.Locator("#mobile-navigation .beeday-brand"));
        await Expect(Page.Locator("#mobile-navigation")).ToBeVisibleAsync();
    }

    private async Task AssertGlobalFoundationAsync()
    {
        var body = Page.Locator("body");
        var fontFamily = await body.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");
        var backgroundImage = await body.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage");

        Assert.Contains("Nunito", fontFamily, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("none", backgroundImage);
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    private async Task AssertWordmarkAsync(ILocator brand)
    {
        await Expect(brand).ToBeVisibleAsync();
        await Expect(brand).ToHaveAttributeAsync("role", "img");
        await Expect(brand).ToHaveAttributeAsync("aria-label", "beeday");
        await Expect(brand.Locator(".beeday-brand__bee")).ToHaveTextAsync("bee");
        await Expect(brand.Locator(".beeday-brand__day")).ToHaveTextAsync("day");
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-foundations-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
