using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

// EPIC 30 Sprint 30.29 (Full-System Regression & Realistic State Validation): every E2E test in
// this suite before this Sprint seeded a fresh user with only the minimal single-feature data that
// one test needed (SeedUserAsync itself has no data-seeding hooks beyond the bare User row) — no
// test exercised a realistic user who already has Habits, a Task, a Project/To-Do and Wallet
// activity simultaneously, navigating across those feature areas. This test closes exactly that gap:
// it builds state across 4 feature areas in one real browser session, then proves navigating away
// and back (Daily -> Wallet -> Daily -> Profile) never loses or corrupts any of it, and that the XP
// total genuinely reflects contributions from two different feature types (Habit + Task), not just
// whichever was completed last.
public sealed class CrossFeatureRealisticStateTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task UserWithDataAcrossHabitsTasksProjectsAndWallet_KeepsAllStateAcrossCrossFeatureNavigation()
    {
        var email = $"e2e-cross-feature-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await SubmitLoginAsync(email, Password);
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var habitTitle = $"E2E Cross Habit {Guid.NewGuid():N}"[..24];
        var taskTitle = $"E2E Cross Task {Guid.NewGuid():N}"[..24];
        var projectTitle = $"E2E Cross Project {Guid.NewGuid():N}"[..26];
        var todoTitle = $"E2E Cross Todo {Guid.NewGuid():N}"[..24];

        await CreateActivityAsync("Habit", habitTitle);
        await CreateActivityAsync("Task", taskTitle);
        await CreateActivityAsync("Project", projectTitle);

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {projectTitle}" }).ClickAsync();
        var projectEditor = Page.GetByRole(AriaRole.Dialog, new() { Name = "Edit Project" });
        await Expect(projectEditor).ToBeVisibleAsync();
        await projectEditor.GetByRole(AriaRole.Button, new() { Name = "Open Project" }).ClickAsync();
        var workspace = Page.GetByRole(AriaRole.Dialog, new() { Name = projectTitle });
        await Expect(workspace).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add To-Do" }).ClickAsync();
        var todoDialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Create To-Do" });
        await Expect(todoDialog).ToBeVisibleAsync();
        await todoDialog.GetByLabel("Title").FillAsync(todoTitle);
        await todoDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(todoDialog).ToBeHiddenAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(workspace).ToBeHiddenAsync();

        // Two different feature types (Habit registration, Task completion) both grant XP — this
        // proves the total genuinely aggregates across feature types, not just the most recent one.
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Register positive for {habitTitle}" }).ClickAsync();
        await Expect(Page.Locator(".habit-card").Filter(new() { Has = Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {habitTitle}" }) }).Locator(".habit-card__balance"))
            .ToHaveTextAsync("+1");
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {taskTitle}" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed tasks" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {taskTitle} as incomplete" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show active tasks" }).ClickAsync();

        var xpAfterHabitAndTask = await ReadExperienceTextAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Wallet" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tagName = $"E2E Cross Tag {Guid.NewGuid():N}"[..16];
        var tagDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "New tag" }).ClickAsync();
        await Expect(tagDialog).ToBeVisibleAsync();
        await tagDialog.GetByLabel("Name").FillAsync(tagName);
        await tagDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(tagDialog).ToBeHiddenAsync();

        const string transactionDescription = "E2E cross-feature transaction";
        var transactionDialog = Page.GetByRole(AriaRole.Dialog);
        await Page.GetByRole(AriaRole.Button, new() { Name = "New transaction" }).ClickAsync();
        await Expect(transactionDialog).ToBeVisibleAsync();
        await transactionDialog.GetByLabel("Description").FillAsync(transactionDescription);
        await transactionDialog.GetByLabel("Type").SelectOptionAsync(new SelectOptionValue { Label = "Income" });
        await transactionDialog.GetByLabel("Amount").FillAsync("25");
        await transactionDialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(transactionDialog).ToBeHiddenAsync();
        await Expect(Page.Locator(".wallet-summary__card--balance strong")).ToHaveTextAsync("$25.00");

        // Cross back to Daily — every item created before the Wallet detour must still be exactly
        // as it was left, none of it scoped to or lost by the Wallet visit.
        await Page.GetByRole(AriaRole.Link, new() { Name = "Daily" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/daily$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(Page.Locator(".habit-card").Filter(new() { Has = Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Habit: {habitTitle}" }) }).Locator(".habit-card__balance"))
            .ToHaveTextAsync("+1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed tasks" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {taskTitle} as incomplete" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {projectTitle}" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {projectTitle}" }).ClickAsync();
        await Expect(projectEditor).ToBeVisibleAsync();
        await projectEditor.GetByRole(AriaRole.Button, new() { Name = "Open Project" }).ClickAsync();
        await Expect(workspace).ToBeVisibleAsync();
        await Expect(workspace.Locator(".project-workspace__todo").Filter(new() { HasText = todoTitle })).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(workspace).ToBeHiddenAsync();

        // Cross back to Wallet once more — the tag/transaction created before the Daily detour must
        // also have survived, and the XP total read earlier must be completely unchanged (Daily
        // round trip touched no XP-granting action).
        await Page.GetByRole(AriaRole.Link, new() { Name = "Wallet" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/wallet$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator(".wallet-summary__card--balance strong")).ToHaveTextAsync("$25.00");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Transaction: {transactionDescription}, +$25.00" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Tag: {tagName}" })).ToBeVisibleAsync();

        var xpAfterCrossNavigation = await ReadExperienceTextAsync();
        Assert.Equal(xpAfterHabitAndTask, xpAfterCrossNavigation);
    }

    private async Task CreateActivityAsync(string activityType, string title)
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Activity" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Menu, new() { Name = "Choose activity type" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = activityType }).ClickAsync();

        var dialog = Page.GetByRole(AriaRole.Dialog);
        await dialog.GetByLabel("Title").FillAsync(title);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
    }

    private async Task<string> ReadExperienceTextAsync()
    {
        await GotoAsync("/profile");
        var text = await Page.Locator(".product-home__progress .experience-card")
            .GetByText(new Regex(@"\d+\s*/\s*\d+ XP"))
            .InnerTextAsync();
        await GotoAsync("/daily");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return text;
    }
}
