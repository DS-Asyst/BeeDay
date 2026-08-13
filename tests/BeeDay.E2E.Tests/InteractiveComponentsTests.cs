using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>Epic 21 Sprint 21.5 controls validated through Chromium computed states.</summary>
public sealed class InteractiveComponentsTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task LoginButtonAndFieldsUsePhysicalSharedControlsWithoutLayoutShift()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/login");

        var email = Page.GetByLabel("Email");
        var button = Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
        await Expect(button).ToBeVisibleAsync();

        Assert.Equal("2px", await email.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.Equal("12px", await email.EvaluateAsync<string>("element => getComputedStyle(element).borderRadius"));
        Assert.Equal("44px", await button.EvaluateAsync<string>("element => getComputedStyle(element).height"));
        Assert.Equal("4px", await button.EvaluateAsync<string>("element => getComputedStyle(element).borderBottomWidth"));
        Assert.Equal("none", await button.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));

        var box = await button.BoundingBoxAsync();
        Assert.NotNull(box);
        await Page.Mouse.MoveAsync((float)(box!.X + box.Width / 2), (float)(box.Y + box.Height / 2));
        await Page.Mouse.DownAsync();
        Assert.Equal("0px", await button.EvaluateAsync<string>("element => getComputedStyle(element).borderBottomWidth"));
        Assert.NotEqual("none", await button.EvaluateAsync<string>("element => getComputedStyle(element).transform"));
        await Page.Mouse.UpAsync();
        Assert.Equal("4px", await button.EvaluateAsync<string>("element => getComputedStyle(element).borderBottomWidth"));

        await button.FocusAsync();
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Page.Keyboard.PressAsync("Tab");
        Assert.True(await button.EvaluateAsync<bool>("element => element.matches(':focus-visible')"));
        Assert.Equal("solid", await button.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
    }

    [Fact]
    public async Task DailyEditorWalletAndMobileControlsRemainUsableWithSharedTargets()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();

        var createButton = Page.GetByRole(AriaRole.Button, new() { Name = "Activity" });
        Assert.True(await createButton.EvaluateAsync<bool>("element => element.getBoundingClientRect().height >= 36"));
        await createButton.ClickAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();

        var editor = Page.GetByRole(AriaRole.Dialog);
        await Expect(editor).ToBeVisibleAsync();
        Assert.Equal("2px", await editor.GetByLabel("Title").EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        await editor.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Wallet" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "New transaction" })).ToBeVisibleAsync();

        await Page.SetViewportSizeAsync(390, 844);
        var menu = Page.GetByRole(AriaRole.Button, new() { Name = "Open navigation menu" });
        Assert.True(await menu.EvaluateAsync<bool>("element => element.getBoundingClientRect().height >= 40"));
        await menu.ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Close navigation menu" })).ToBeVisibleAsync();
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-controls-{Guid.NewGuid():N}@beeday.invalid";
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
