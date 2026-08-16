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

    [Theory]
    [InlineData(320, 568)]
    [InlineData(390, 844)]
    public async Task PasswordRecoveryUsesSharedFieldWithoutHorizontalOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/account/forgot-password");

        var email = Page.GetByLabel("Email");
        await Expect(email).ToBeVisibleAsync();
        await Expect(email).ToHaveClassAsync(new Regex("beeday-field__control"));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send recovery link" }).ClickAsync();
        await Expect(Page.Locator(".beeday-validation-message[role='alert']")).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(768, 900)]
    [InlineData(1280, 800)]
    public async Task CreateAccountMatchesPublicAuthenticationLayout(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/profile/create");

        await Expect(Page.Locator(".profile-panel")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Close create account and return to Home" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Log in" })).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Full name")).ToHaveClassAsync(new Regex("beeday-field__control"));
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Continue" })).ToBeDisabledAsync();
        Assert.Equal("rgba(0, 0, 0, 0)", await Page.Locator(".profile-panel")
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task CreateAccountTopActionsNavigateExplicitly()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/profile/create");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/login$"));

        await GotoAsync("/profile/create");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Close create account and return to Home" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/$"));
    }
}
