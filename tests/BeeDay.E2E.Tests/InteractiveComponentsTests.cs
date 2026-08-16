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
        Assert.Equal("52px", await button.EvaluateAsync<string>("element => getComputedStyle(element).height"));
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

    [Fact]
    public async Task NestedDialogsTrapKeyboardAndRestoreFocusAcrossEscapeClosures()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await LoginToDailyAsync();
        var title = $"Focus lifecycle {Guid.NewGuid():N}"[..30];

        await Page.GetByRole(AriaRole.Button, new() { Name = "Activity" }).ClickAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();

        var editor = Page.GetByRole(AriaRole.Dialog);
        var titleInput = editor.GetByLabel("Title");
        await Expect(titleInput).ToBeFocusedAsync();
        await titleInput.FillAsync(title);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();

        var editTrigger = Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {title}" });
        await editTrigger.ClickAsync();
        editor = Page.GetByRole(AriaRole.Dialog);
        await Expect(editor.GetByLabel("Title")).ToBeFocusedAsync();

        var delete = editor.GetByRole(AriaRole.Button, new() { Name = "Delete" });
        await delete.ClickAsync();

        var confirmation = Page.Locator("[role='alertdialog']");
        var cancel = confirmation.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
        var confirm = confirmation.GetByRole(AriaRole.Button, new() { Name = "Delete Task" });
        await Expect(cancel).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(confirm).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(cancel).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(confirmation).ToBeHiddenAsync();
        await Expect(delete).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(editor).ToBeHiddenAsync();
        await Expect(editTrigger).ToBeFocusedAsync();
    }

    [Fact]
    public async Task FocusScopeHandlesDialogWithoutControlsAndRemovedTrigger()
    {
        await GotoAsync("/login");

        var handled = await Page.EvaluateAsync<bool>("""
            async () => {
                const trigger = document.querySelector('#login-email');
                const dialog = document.createElement('section');
                dialog.id = 'e2e-empty-focus-scope';
                dialog.tabIndex = -1;
                document.body.append(dialog);

                const focusScope = await import('/js/beeday-dialog-focus.js?v=20260816-1');
                trigger.focus();
                focusScope.activate(dialog.id, 'self');
                await Promise.resolve();

                const focusedEmptyDialog = document.activeElement === dialog;
                trigger.remove();

                try {
                    focusScope.deactivate(dialog.id);
                } catch {
                    dialog.remove();
                    return false;
                }

                dialog.remove();
                return focusedEmptyDialog;
            }
            """);

        Assert.True(handled);
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-controls-{Guid.NewGuid():N}@beeday.invalid";
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
