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

    // EPIC 30 Sprint 30.12 (BD30-F049 pattern continued): the only prior Habit E2E coverage was
    // create + register-positive. Edit, delete, and register-negative had no browser-level coverage
    // anywhere in the suite.
    [Fact]
    public async Task RegisterNegativeForHabit_UpdatesBalance()
    {
        await LoginToDailyAsync();
        var title = $"E2E Habit Neg {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Habit" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        var card = Page.Locator(".habit-card").Filter(new() { Has = Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" }) });
        await Expect(card.Locator(".habit-card__balance")).ToHaveTextAsync("0");

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Register negative for {title}" }).ClickAsync();
        await Expect(card.Locator(".habit-card__balance")).ToHaveTextAsync("-1");
    }

    [Fact]
    public async Task EditHabit_UpdatesTitleAndPersistsAfterReload()
    {
        await LoginToDailyAsync();
        var title = $"E2E Habit Edit {Guid.NewGuid():N}"[..24];
        var updatedTitle = $"E2E Habit Edited {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Habit" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" }).ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByLabel("Title").FillAsync(updatedTitle);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {updatedTitle}" })).ToBeVisibleAsync();

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {updatedTitle}" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task DeleteHabit_RemovesItFromTheBoardAfterConfirmation()
    {
        await LoginToDailyAsync();
        var title = $"E2E Habit Delete {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Habit" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" }).ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();

        var confirmation = Page.GetByRole(AriaRole.Alertdialog);
        await Expect(confirmation).ToBeVisibleAsync();
        await confirmation.GetByRole(AriaRole.Button, new() { Name = "Delete Habit" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" })).ToHaveCountAsync(0);

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {title}" })).ToHaveCountAsync(0);
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

    // EPIC 30 Sprint 30.13: mirrors the Habit edit/delete E2E tests added in Sprint 30.12
    // (EditHabit_UpdatesTitleAndPersistsAfterReload / DeleteHabit_RemovesItFromTheBoardAfterConfirmation)
    // — Task previously had E2E coverage only for create + toggle-complete.
    [Fact]
    public async Task EditTask_UpdatesTitleAndPersistsAfterReload()
    {
        await LoginToDailyAsync();
        var title = $"E2E Task Edit {Guid.NewGuid():N}"[..24];
        var updatedTitle = $"E2E Task Edited {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {title}" }).ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByLabel("Title").FillAsync(updatedTitle);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {updatedTitle}" })).ToBeVisibleAsync();

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {updatedTitle}" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {title}" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task DeleteTask_RemovesItFromTheBoardAfterConfirmation()
    {
        await LoginToDailyAsync();
        var title = $"E2E Task Delete {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {title}" }).ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();

        var confirmation = Page.GetByRole(AriaRole.Alertdialog);
        await Expect(confirmation).ToBeVisibleAsync();
        await confirmation.GetByRole(AriaRole.Button, new() { Name = "Delete Task" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {title}" })).ToHaveCountAsync(0);

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Task: {title}" })).ToHaveCountAsync(0);
    }

    // EPIC 30 Sprint 30.16: the only prior XP-visibility E2E coverage was Habit
    // (CreateAndCompleteHabit_UpdatesBalanceAndXp) — Task, Todo, and Project completion never had
    // browser-level proof that completing them visibly grants XP.
    [Fact]
    public async Task CompleteTask_UpdatesXp()
    {
        await LoginToDailyAsync();
        var title = $"E2E Task Xp {Guid.NewGuid():N}"[..24];

        var xpBefore = await ReadExperienceTextAsync();

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed tasks" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {title} as incomplete" })).ToBeVisibleAsync();

        var xpAfter = await ReadExperienceTextAsync();
        Assert.NotEqual(xpBefore, xpAfter);
    }

    // EPIC 30 Sprint 30.16: the level-up modal (BeeDayFeedbackModal) previously had bUnit coverage
    // only (BeeDayFeedbackTests.cs) — never verified end-to-end through a real handler execution,
    // real domain-event publish, and real Blazor Server render. Seeding the user 5 XP below the
    // Level 2 threshold (100, per LinearExperienceCurve's documented/tested boundary) means the
    // single Task completion below (5 XP, ExperienceRewardPolicy) crosses it exactly.
    [Fact]
    public async Task CompleteTask_AtALevelBoundary_ShowsTheLevelUpModalExactlyOnce()
    {
        await LoginToDailyAsync(initialExperience: 95);
        var title = $"E2E Task LevelUp {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var editor = Page.GetByRole(AriaRole.Dialog);
        await editor.GetByLabel("Title").FillAsync(title);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" }).ClickAsync();

        var levelUpModal = Page.GetByRole(AriaRole.Dialog, new() { Name = "BE BETTER EVERY DAY" });
        await Expect(levelUpModal).ToBeVisibleAsync();
        await Expect(levelUpModal.GetByTestId("previous-level")).ToHaveTextAsync("1");
        await Expect(levelUpModal.GetByTestId("new-level")).ToHaveTextAsync("2");

        await levelUpModal.GetByRole(AriaRole.Button, new() { Name = "Continue" }).ClickAsync();
        await Expect(levelUpModal).ToBeHiddenAsync();

        // A full reload starts a new circuit with a fresh, empty feedback store — the modal must
        // not reappear for an already-consumed level-up.
        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "BE BETTER EVERY DAY" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task TaskCheckbox_HoverNeverPreviewsTheCheckBeforeCompletion()
    {
        // Sprint 29.3: EPIC 27 Sprint 27.10 originally made hover fade the check glyph to partial
        // opacity as a "preview" — that read as a false completed affordance on an item that hadn't
        // actually been completed yet, so it was removed. Hover may still change the checkbox's own
        // surface/border for affordance, but the glyph itself must stay fully hidden until the item
        // is genuinely completed.
        await LoginToDailyAsync();
        var title = $"E2E Hover {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        var checkbox = Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" });
        var glyph = checkbox.Locator(".activity-card__checkbox-glyph");

        Assert.Equal("0", await glyph.EvaluateAsync<string>("element => getComputedStyle(element).opacity"));

        await checkbox.HoverAsync();
        await Page.WaitForTimeoutAsync(200);
        Assert.Equal("0", await glyph.EvaluateAsync<string>("element => getComputedStyle(element).opacity"));

        // Hovering alone must never have completed it.
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" })).ToBeVisibleAsync();

        await checkbox.ClickAsync();

        // Completing moves the task out of the Active view (see CreateAndCompleteTask_TogglesCompletion).
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed tasks" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {title} as incomplete" })).ToBeVisibleAsync();

        // Completed must stay visibly checked after the mouse moves away.
        await Page.Mouse.MoveAsync(5, 5);
        var completedCheckbox = Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {title} as incomplete" });
        Assert.Equal("1", await completedCheckbox.Locator(".activity-card__checkbox-glyph").EvaluateAsync<string>("element => getComputedStyle(element).opacity"));
    }

    [Fact]
    public async Task TaskCheckbox_CompletesViaKeyboardAloneWithoutAnyHover()
    {
        // EPIC 27 Sprint 27.10 acceptance: "Teclado/touch conseguem concluir sem depender de hover."
        await LoginToDailyAsync();
        var title = $"E2E Keyboard {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Task" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        var checkbox = Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {title}" });
        await checkbox.FocusAsync();
        await Expect(checkbox).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Completing moves the task out of the Active view (see CreateAndCompleteTask_TogglesCompletion).
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

        // Sprint 29.3: Open Project used to opt into Compact="true", rendering shorter/smaller-text
        // than the Cancel button sitting right next to it in the same footer row — this protects
        // the fix by comparing their real computed heights instead of just a class assertion.
        var openProjectButton = editor.GetByRole(AriaRole.Button, new() { Name = "Open Project" });
        var cancelButton = editor.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
        Assert.Equal(
            await cancelButton.EvaluateAsync<string>("element => getComputedStyle(element).height"),
            await openProjectButton.EvaluateAsync<string>("element => getComputedStyle(element).height"));

        await openProjectButton.ClickAsync();

        var workspace = Page.GetByRole(AriaRole.Dialog, new() { Name = title });
        await Expect(workspace).ToBeVisibleAsync();
        await Expect(workspace.GetByRole(AriaRole.Button, new() { Name = "Close project" })).ToBeFocusedAsync();
        await Expect(workspace.GetByRole(AriaRole.Progressbar, new() { Name = "Project progress 0%" })).ToBeVisibleAsync();

        await Page.SetViewportSizeAsync(390, 844);
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));

        await Page.Keyboard.PressAsync("Escape");
        await Expect(workspace).ToBeHiddenAsync();
    }

    [Fact]
    public async Task ProjectCard_ShowsTheMiniBeeIndicatorInsteadOfTheOldCoinLikeIcon()
    {
        // EPIC 27 Sprint 27.10 acceptance: "Projects não exibem mais a moeda antiga no indicador alvo."
        await LoginToDailyAsync();
        var title = $"E2E Project Badge {Guid.NewGuid():N}"[..26];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Project" }).ClickAsync();
        var editor = Page.GetByRole(AriaRole.Dialog);
        await editor.GetByLabel("Title").FillAsync(title);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();

        var card = Page.Locator(".activity-card--project").Filter(new() { HasText = title });
        var badge = card.Locator(".activity-card__project-status");
        await Expect(badge).ToHaveAttributeAsync("src", new Regex("project-bee\\.png$"));
        await Expect(card.Locator("svg.beeday-icon")).ToHaveCountAsync(0);
    }

    // EPIC 30 Sprint 30.24 (BD30-F083): beeday-sortable.js has implemented an ArrowUp/ArrowDown
    // keyboard alternative to drag-and-drop reordering (same NotifyReorderAsync path) since it was
    // written, but no E2E test anywhere in the suite ever exercised it — a real, accessible
    // capability with zero browser-level proof it actually works end to end.
    [Fact]
    public async Task ArrowDown_ReordersHabitsAndPersistsAfterReload()
    {
        await LoginToDailyAsync();
        var titleA = $"E2E Reorder A {Guid.NewGuid():N}"[..24];
        var titleB = $"E2E Reorder B {Guid.NewGuid():N}"[..24];

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Habit" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(titleA);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await OpenActivityMenuAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Habit" }).ClickAsync();
        await dialog.GetByLabel("Title").FillAsync(titleB);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        var items = Page.Locator("[data-sortable-key='habits'] [data-sortable-item]");
        await Expect(items).ToHaveCountAsync(2);
        await Expect(items.Nth(0)).ToContainTextAsync(titleA);
        await Expect(items.Nth(1)).ToContainTextAsync(titleB);

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {titleA}" }).FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(items.Nth(0)).ToContainTextAsync(titleB);
        await Expect(items.Nth(1)).ToContainTextAsync(titleA);

        await GotoAsync("/daily");
        var itemsAfterReload = Page.Locator("[data-sortable-key='habits'] [data-sortable-item]");
        await Expect(itemsAfterReload.Nth(0)).ToContainTextAsync(titleB);
        await Expect(itemsAfterReload.Nth(1)).ToContainTextAsync(titleA);
    }

    private async Task LoginToDailyAsync(long? initialExperience = null)
    {
        var email = $"e2e-activity-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true, initialExperience: initialExperience);

        await SubmitLoginAsync(email, Password);
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
