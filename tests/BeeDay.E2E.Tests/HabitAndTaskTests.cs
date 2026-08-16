using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Habit and Task creation/completion as seen in the browser, including that completing an
/// activity visibly updates XP — without validating the XP calculation itself (that's Domain/
/// Application unit-test territory, already covered).
/// </summary>
public sealed class HabitAndTaskTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task CreateAndCompleteHabit_UpdatesBalanceAndXp()
    {
        await LoginToDailyAsync();
        var title = $"E2E Habit {Guid.NewGuid():N}"[..24];

        var xpBefore = await ReadExperienceTextAsync();

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Habit" }).ClickAsync();

        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        var card = Page.Locator(".habit-card").Filter(new() { Has = Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" }) });
        await Expect(card).ToBeVisibleAsync();
        await Expect(card.Locator(".habit-card__balance")).ToHaveTextAsync("0");

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Register positive for {title}" }).ClickAsync();
        await Expect(card.Locator(".habit-card__balance")).ToHaveTextAsync("+1");

        var xpAfter = await ReadExperienceTextAsync();
        Assert.NotEqual(xpBefore, xpAfter);
    }

    [Fact]
    public async Task CreateAndCompleteTask_TogglesCompletion()
    {
        await LoginToDailyAsync();
        var title = $"E2E Task {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();

        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        var completeButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" });
        await Expect(completeButton).ToBeVisibleAsync();
        await completeButton.ClickAsync();

        // Completing a task moves it out of DashboardColumn's Active view into its Completed
        // view, which only renders once the column's own view toggle is switched — the Active
        // view then shows its empty state in place of the card, it does not update in place like
        // a Habit's balance does. The toggle must be clicked before the completed card appears.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed tasks" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {title} as incomplete" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectWorkspace_UsesSharedProgressFocusAndResponsiveContracts()
    {
        await LoginToDailyAsync();
        var title = $"E2E Project {Guid.NewGuid():N}"[..26];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Project" }).ClickAsync();

        var editor = Page.GetByRole(AriaRole.Dialog);
        await editor.GetByLabel("Title").FillAsync(title);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {title}" }).ClickAsync();
        await Expect(editor).ToBeVisibleAsync();
        await editor.GetByRole(AriaRole.Button, new() { Name = "Open Project" }).ClickAsync();

        var workspace = Page.GetByRole(AriaRole.Dialog, new() { Name = title });
        await Expect(workspace).ToBeVisibleAsync();
        await Expect(workspace.GetByRole(AriaRole.Button, new() { Name = "Close project" })).ToBeFocusedAsync();
        await Expect(workspace.GetByRole(AriaRole.Progressbar, new() { Name = "Project progress 0%" })).ToBeVisibleAsync();

        await Page.SetViewportSizeAsync(390, 844);
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));

        await Page.Keyboard.PressAsync("Escape");
        await Expect(workspace).ToBeHiddenAsync();
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-activity-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");

        // The Sign In click triggers a real server-side redirect to a brand new page (/daily),
        // which establishes its own SignalR circuit; GotoAsync's network-idle wait only covers
        // explicit navigations, not ones reached via a redirect, so it has to be repeated here
        // before any interactive (non-form-post) click on the destination page.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<string> ReadExperienceTextAsync()
    {
        await GotoAsync("/profile");
        var text = await Page.Locator(".product-home__progress .experience-card")
            .GetByText(new Regex(@"\d+\s*/\s*\d+ XP"))
            .InnerTextAsync();
        await GotoAsync("/daily");
        return text;
    }

    /// <summary>
    /// Opens the "Activity" create menu and confirms it is genuinely open. The menu items are
    /// conditionally rendered (an @if block, not CSS-hidden), so waiting for the menu container's
    /// own role/label is a reliable, render-confirmed signal that the click was received and the
    /// interactive render completed. The trigger also exposes lowercase aria-expanded, covered by
    /// the component accessibility contract.
    /// </summary>
    private async Task OpenActivityMenuAsync()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Activity" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Menu, new() { Name = "Choose activity type" })).ToBeVisibleAsync();
    }
}
