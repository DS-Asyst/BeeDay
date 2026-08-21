using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// EPIC 30 Sprint 30.11 / BD30-F047: LoginExperienceTests already proves /profile/create Step 1
/// (the anonymous Account form) is overflow-free at mobile/tablet/desktop widths, but nothing
/// exercised Step 2 (the nickname form), /account (all three settings sections), or
/// /onboarding/tutorial — the three other critical forms in the Profile/Onboarding/Account journey.
/// Mirrors LoginExperienceTests' pattern: a real viewport, a real render, and the same
/// scrollWidth-vs-clientWidth check used there.
/// </summary>
public sealed class AccountResponsiveLayoutTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Theory]
    [InlineData(390, 844)]
    [InlineData(1280, 800)]
    public async Task CreateProfileNicknameStepUsesViewportWithoutHorizontalOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/profile/create");

        await Page.GetByLabel("Full name").FillAsync("E2E Responsive User");
        await Page.GetByLabel("Email").FillAsync($"e2e-responsive-{Guid.NewGuid():N}@beeday.invalid");
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(Password);
        await Page.GetByLabel("Confirm password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).ClickAsync();

        var nickname = Page.GetByLabel("Nickname");
        await Expect(nickname).ToBeVisibleAsync();
        await Expect(nickname).ToHaveClassAsync(new Regex("beeday-field__control"));
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create account" })).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(1280, 800)]
    public async Task AccountSettingsUsesViewportWithoutHorizontalOverflow(int width, int height)
    {
        var email = $"e2e-responsive-account-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await Page.SetViewportSizeAsync(width, height);
        await SubmitLoginAsync(email, Password);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GotoAsync("/account");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "My Account" })).ToBeVisibleAsync();

        await Expect(Page.GetByLabel(new Regex("^Name\\*?$"))).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save Profile" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Change Password" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save Preferences" })).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(1280, 800)]
    public async Task OnboardingTutorialUsesViewportWithoutHorizontalOverflow(int width, int height)
    {
        var email = $"e2e-responsive-tutorial-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: false);

        await Page.SetViewportSizeAsync(width, height);
        await SubmitLoginAsync(email, Password);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(Page).ToHaveURLAsync(new Regex("/onboarding/tutorial$"));
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "NEXT" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "BACK" })).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
