using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class LoginExperienceTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task TopActionsNavigateExplicitlyToHomeAndRegistration()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/login");

        var close = Page.GetByRole(AriaRole.Link, new() { Name = "Close login and return to Home" });
        await Expect(close).ToBeVisibleAsync();
        await close.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/$"));

        await GotoAsync("/login");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create account" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile/create$"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(1280, 800)]
    public async Task LoginUsesViewportWithoutCardOrHorizontalOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/login");

        await Expect(Page.Locator(".auth-login")).ToBeVisibleAsync();
        await Expect(Page.Locator(".auth-card")).ToHaveCountAsync(0);
        await Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Password")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Remember me")).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task KeyboardOrderStartsWithTopActionsAndKeepsVisibleFocus()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/login");

        var close = Page.GetByRole(AriaRole.Link, new() { Name = "Close login and return to Home" });
        await close.FocusAsync();
        await Expect(close).ToBeFocusedAsync();
        Assert.Equal("solid", await close.EvaluateAsync<string>(
            "element => getComputedStyle(element).outlineStyle"));

        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Create account" })).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.GetByLabel("Email")).ToBeFocusedAsync();
    }
}
