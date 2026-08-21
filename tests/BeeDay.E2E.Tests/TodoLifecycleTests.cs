using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// EPIC 30 Sprint 30.13: closes BD30-F019 — before this file, To-Do had zero E2E coverage. Create,
/// toggle, edit, and delete were only ever proven at the Application-handler and bUnit-modal level.
/// A To-Do always belongs to a Project, so every journey here first creates one and opens its
/// workspace (the only place "Add To-Do" exists) before the To-Do itself is manipulated from the
/// Dashboard board, exactly like a real user would.
/// </summary>
public sealed class TodoLifecycleTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task CreateTodoInsideAProject_AppearsOnTheBoardAndTogglesComplete()
    {
        await LoginToDailyAsync();
        var projectTitle = $"E2E Project {Guid.NewGuid():N}"[..24];
        var todoTitle = $"E2E Todo {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(projectTitle);
        await using (await OpenProjectWorkspaceAsync(projectTitle))
        {
            await CreateTodoInOpenWorkspaceAsync(todoTitle);
        }

        var completeButton = Page.GetByRole(AriaRole.Button, new() { Name = $"Complete {todoTitle}" });
        await Expect(completeButton).ToBeVisibleAsync();
        await completeButton.ClickAsync();

        // Mirrors CreateAndCompleteTask_TogglesCompletion: completing moves the item out of the
        // Active view into the Completed view, which only renders once the toggle is switched.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show completed to-dos" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Mark {todoTitle} as incomplete" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task EditTodo_UpdatesTitleAndPersistsAfterReload()
    {
        await LoginToDailyAsync();
        var projectTitle = $"E2E Project Edit {Guid.NewGuid():N}"[..24];
        var todoTitle = $"E2E Todo Edit {Guid.NewGuid():N}"[..24];
        var updatedTodoTitle = $"E2E Todo Edited {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(projectTitle);
        await using (await OpenProjectWorkspaceAsync(projectTitle))
        {
            await CreateTodoInOpenWorkspaceAsync(todoTitle);
        }

        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Edit To-Do" });
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {todoTitle}" }).ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByLabel("Title").FillAsync(updatedTodoTitle);
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(dialog).ToBeHiddenAsync();

        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {updatedTodoTitle}" })).ToBeVisibleAsync();

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {updatedTodoTitle}" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {todoTitle}" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task DeleteTodo_RemovesItFromTheBoardAfterConfirmation()
    {
        await LoginToDailyAsync();
        var projectTitle = $"E2E Project Delete {Guid.NewGuid():N}"[..24];
        var todoTitle = $"E2E Todo Delete {Guid.NewGuid():N}"[..24];

        await CreateProjectAsync(projectTitle);
        await using (await OpenProjectWorkspaceAsync(projectTitle))
        {
            await CreateTodoInOpenWorkspaceAsync(todoTitle);
        }

        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Edit To-Do" });
        await Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {todoTitle}" }).ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete", Exact = true }).ClickAsync();

        var confirmation = Page.GetByRole(AriaRole.Alertdialog);
        await Expect(confirmation).ToBeVisibleAsync();
        await confirmation.GetByRole(AriaRole.Button, new() { Name = "Delete To-Do" }).ClickAsync();

        await Expect(dialog).ToBeHiddenAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {todoTitle}" })).ToHaveCountAsync(0);

        await GotoAsync("/daily");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = $"Edit To-Do: {todoTitle}" })).ToHaveCountAsync(0);
    }

    private async Task LoginToDailyAsync()
    {
        var email = $"e2e-todo-{Guid.NewGuid():N}@beeday.invalid";
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
    /// Opens the Project workspace (the only surface with an "Add To-Do" control) and returns an
    /// <see cref="IAsyncDisposable"/> that closes it again via Escape — mirrors
    /// ProjectWorkspace_UsesSharedProgressFocusAndResponsiveContracts' own close pattern, wrapped so
    /// callers can scope workspace-only interaction with a `using` block.
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
