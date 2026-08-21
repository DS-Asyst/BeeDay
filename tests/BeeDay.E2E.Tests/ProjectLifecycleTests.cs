using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// EPIC 30 Sprint 30.14: closes the remaining half of BD30-F020. Sprint 30.13's TodoLifecycleTests
/// already proved Todo mutations inside an open Project workspace persist after a real reload, but
/// always reopened the board (never the workspace itself) to check — the workspace's own state
/// (progress bar, embedded To-Do list) surviving a reload, editing a Project's own fields, and
/// deleting a Project were all still unproven E2E before this file.
/// </summary>
public sealed class ProjectLifecycleTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task ProjectWorkspace_ProgressAndTodoListSurviveARealReload()
    {
        await LoginToDailyAsync();
        var projectTitle = $"E2E Project Progress {Guid.NewGuid():N}"[..24];
        var doneTodoTitle = $"E2E Todo Done {Guid.NewGuid():N}"[..24];
        var openTodoTitle = $"E2E Todo Open {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(projectTitle);
        await using (await OpenProjectWorkspaceAsync(projectTitle))
        {
            await CreateTodoInOpenWorkspaceAsync(doneTodoTitle);
            await CreateTodoInOpenWorkspaceAsync(openTodoTitle);
        }

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {doneTodoTitle}" }).ClickAsync();

        // The toggle click only dispatches the DOM event — it does not wait for the Blazor Server
        // round-trip that actually persists it. Waiting for the button's own accessible name to flip
        // is what actually confirms the mutation committed, before navigating away below; without
        // this, GotoAsync can race ahead of the server-side toggle and reload before it lands.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed to-dos" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {doneTodoTitle} as incomplete" })).ToBeVisibleAsync();

        // A real full-page navigation, not an interactive Blazor update — proves persistence, not
        // just in-memory state surviving because the circuit never actually reloaded.
        await GotoAsync("/daily");

        await using (await OpenProjectWorkspaceAsync(projectTitle))
        {
            var workspace = Page.GetByRole(AriaRole.Dialog, new() { Name = projectTitle });
            await Expect(workspace.GetByRole(AriaRole.Progressbar, new() { Name = "Project progress 50%" })).ToBeVisibleAsync();

            // The To-Dos list starts expanded (showTodos defaults to true) whenever the project has
            // any To-Dos, so it's already visible here — no toggle click needed.
            var done = workspace.Locator(".project-workspace__todo--done");
            await Expect(done).ToHaveCountAsync(1);
            await Expect(done).ToContainTextAsync(doneTodoTitle);
            await Expect(workspace.Locator(".project-workspace__todo").Filter(new() { HasText = openTodoTitle })).Not.ToHaveClassAsync(new Regex("project-workspace__todo--done"));
        }
    }

    // EPIC 30 Sprint 30.20: the Project workspace panel — the most layout-complex authenticated
    // surface after Wallet, embedding its own To-Do list and progress bar — had zero narrow-viewport
    // overflow coverage anywhere in the suite, unlike /daily, /wallet, /account and the onboarding
    // routes (all covered elsewhere).
    [Fact]
    public async Task ProjectWorkspace_RendersWithoutHorizontalOverflowOnMobile()
    {
        await LoginToDailyAsync();
        var projectTitle = $"E2E Project Mobile {Guid.NewGuid():N}"[..24];
        var todoTitle = $"E2E Todo Mobile {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(projectTitle);
        await using (await OpenProjectWorkspaceAsync(projectTitle))
        {
            await CreateTodoInOpenWorkspaceAsync(todoTitle);

            await Page.SetViewportSizeAsync(390, 844);
            var workspace = Page.GetByRole(AriaRole.Dialog, new() { Name = projectTitle });
            await Expect(workspace.GetByRole(AriaRole.Progressbar)).ToBeVisibleAsync();
            await Expect(workspace.Locator(".project-workspace__todo").Filter(new() { HasText = todoTitle })).ToBeVisibleAsync();
            Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        }
    }

    [Fact]
    public async Task EditProject_UpdatesFieldsAndPersistsAfterReload()
    {
        await LoginToDailyAsync();
        var title = $"E2E Project Edit {Guid.NewGuid():N}"[..24];
        var updatedTitle = $"E2E Project Edited {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(title);

        var editor = Page.GetByRole(AriaRole.Dialog, new() { Name = "Edit Project" });
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {title}" }).ClickAsync();
        await Expect(editor).ToBeVisibleAsync();
        await editor.GetByLabel("Title").FillAsync(updatedTitle);
        await editor.GetByLabel("Notes").FillAsync("Updated scope and notes");
        await editor.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {updatedTitle}" })).ToBeVisibleAsync();

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {updatedTitle}" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {title}" })).ToHaveCountAsync(0);

        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {updatedTitle}" }).ClickAsync();
        await Expect(editor).ToBeVisibleAsync();
        await Expect(editor.GetByLabel("Notes")).ToHaveValueAsync("Updated scope and notes");
    }

    [Fact]
    public async Task DeleteProject_RemovesItFromTheBoardAfterConfirmation()
    {
        await LoginToDailyAsync();
        var title = $"E2E Project Delete {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(title);

        var editor = Page.GetByRole(AriaRole.Dialog, new() { Name = "Edit Project" });
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {title}" }).ClickAsync();
        await Expect(editor).ToBeVisibleAsync();
        await editor.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();

        var confirmation = Page.GetByRole(AriaRole.Alertdialog);
        await Expect(confirmation).ToBeVisibleAsync();
        await confirmation.GetByRole(AriaRole.Button, new() { Name = "Delete Project" }).ClickAsync();

        await Expect(editor).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {title}" })).ToHaveCountAsync(0);

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {title}" })).ToHaveCountAsync(0);
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-project-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await SubmitLoginAsync(email, Password);
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await GotoAsync("/daily");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task CreateProjectAsync(string title)
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Activity" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Menu, new() { Name = "Choose activity type" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Menuitem, new() { Name = "Project" }).ClickAsync();

        var editor = Page.GetByRole(AriaRole.Dialog);
        await editor.GetByLabel("Title").FillAsync(title);
        await editor.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(editor).ToBeHiddenAsync();
    }

    /// <summary>
    /// Opens the Project workspace and returns an <see cref="IAsyncDisposable"/> that closes it
    /// again via Escape — mirrors TodoLifecycleTests' own helper of the same shape, kept local since
    /// E2E test classes in this suite are self-contained (no shared base beyond E2ETestBase).
    /// </summary>
    private async Task<IAsyncDisposable> OpenProjectWorkspaceAsync(string projectTitle)
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit Project: {projectTitle}" }).ClickAsync();
        var editor = Page.GetByRole(AriaRole.Dialog, new() { Name = "Edit Project" });
        await Expect(editor).ToBeVisibleAsync();
        await editor.GetByRole(AriaRole.Button, new() { Name = "Open Project" }).ClickAsync();

        var workspace = Page.GetByRole(AriaRole.Dialog, new() { Name = projectTitle });
        await Expect(workspace).ToBeVisibleAsync();

        return new WorkspaceCloser(Page, workspace);
    }

    private async Task CreateTodoInOpenWorkspaceAsync(string todoTitle)
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add To-Do" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Create To-Do" });
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByLabel("Title").FillAsync(todoTitle);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();
    }

    private sealed class WorkspaceCloser(IPage page, ILocator workspace) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await page.Keyboard.PressAsync("Escape");
            await Expect(workspace).ToBeHiddenAsync();
        }
    }
}
