using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Behaviors.DragDrop;
using LevelUp.Web.Components.Features.Common;
using LevelUp.Web.Components.Features.Habits.Models;
using LevelUp.Web.Components.Features.Projects.Models;
using LevelUp.Web.Components.Features.Tasks.Models;
using LevelUp.Web.Components.Features.Todos.Models;
using LevelUp.Web.Services;

namespace LevelUp.Web.Components.Features.Dashboard.State;

public sealed class DashboardState(LevelUpWebService store, ToastService toastService)
{
    private LevelUpData? data;
    private string search = string.Empty;

    public DashboardModalState Modals { get; } = new();
    public LevelUpData? Data => data;
    public bool IsLoading => data is null;
    public bool IsBusy { get; private set; }
    public Guid? RemovingItemId { get; private set; }
    public event Action? Changed;
    public bool HasProfile => data?.Profile is not null;

    public string Search
    {
        get => search;
        set => search = value ?? string.Empty;
    }

    public int CompletedItems => data is null
        ? 0
        : data.Tasks.Count(item => item.Completed)
          + data.Todos.Count(item => item.Completed)
          + data.Projects.Count(item => item.Completed);

    public int ActiveItems => data is null
        ? 0
        : data.Habits.Count
          + data.Tasks.Count(item => !item.Completed)
          + data.Todos.Count(item => !item.Completed)
          + data.Projects.Count(item => !item.Completed);

    public IEnumerable<Habit> FilteredHabits => data?.Habits.Where(MatchesSearch) ?? [];
    public IEnumerable<RecurringTask> FilteredTasks => data?.Tasks.Where(MatchesSearch) ?? [];
    public IEnumerable<Todo> FilteredTodos => data?.Todos.Where(MatchesSearch) ?? [];
    public IEnumerable<Project> FilteredProjects => data?.Projects.Where(MatchesSearch) ?? [];

    public int TotalItems => data is null
        ? 0
        : data.Habits.Count + data.Tasks.Count + data.Todos.Count + data.Projects.Count;

    public int FilteredItems => FilteredHabits.Count() + FilteredTasks.Count() + FilteredTodos.Count() + FilteredProjects.Count();

    public async Task InitializeAsync()
    {
        try
        {
            await ReloadAsync();
        }
        catch
        {
            toastService.ShowError("The dashboard data could not be loaded. Try refreshing the page.");
            data = new LevelUpData();
        }
    }

    public void OpenCreate(ActivityType type) => Modals.OpenCreate(type);
    public void OpenHabitEditor(Habit item) => Modals.OpenHabit(item);
    public void OpenTaskEditor(RecurringTask item) => Modals.OpenTask(item);
    public void OpenTodoEditor(Todo item) => Modals.OpenTodo(item);
    public void OpenProjectEditor(Project item) => Modals.OpenProject(item);
    public void CloseEditor() => Modals.CloseEditor();
    public void RequestDelete(Guid id, ActivityType type, string title) => Modals.RequestDelete(id, type, title);
    public void CancelDelete() => Modals.CancelDelete();

    public Task SaveHabitAsync(HabitEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateHabitAsync(id, model) : store.AddHabitAsync(model),
            Modals.IsEditing ? "Habit updated successfully." : "Habit created successfully.");

    public Task SaveTaskAsync(TaskEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateTaskAsync(id, model) : store.AddTaskAsync(model),
            Modals.IsEditing ? "Task updated successfully." : "Task created successfully.");

    public Task SaveTodoAsync(TodoEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateTodoAsync(id, model) : store.AddTodoAsync(model),
            Modals.IsEditing ? "To-Do updated successfully." : "To-Do created successfully.");

    public Task SaveProjectAsync(ProjectEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateProjectAsync(id, model) : store.AddProjectAsync(model),
            Modals.IsEditing ? "Project updated successfully." : "Project created successfully.");

    public Task DeleteCurrentHabitAsync() => DeleteCurrentEditorItemAsync(ActivityType.Habit, "Habit deleted successfully.");
    public Task DeleteCurrentTaskAsync() => DeleteCurrentEditorItemAsync(ActivityType.Task, "Task deleted successfully.");
    public Task DeleteCurrentTodoAsync() => DeleteCurrentEditorItemAsync(ActivityType.Todo, "To-Do deleted successfully.");
    public Task DeleteCurrentProjectAsync() => DeleteCurrentEditorItemAsync(ActivityType.Project, "Project deleted successfully.");

    public async Task ConfirmDeleteAsync()
    {
        if (Modals.PendingDeleteId is not Guid id || Modals.PendingDeleteType is not ActivityType type)
        {
            Modals.CancelDelete();
            return;
        }

        var displayName = Modals.DeleteItemDisplayName;
        await ExecuteAsync(
            async () =>
            {
                await AnimateRemovalAsync(id);
                await DeleteAsync(id, type);
                Modals.CancelDelete();
                await ReloadAsync();
            },
            $"{displayName} deleted successfully.",
            $"The {displayName.ToLowerInvariant()} could not be deleted.");
    }

    public Task RegisterPositiveAsync(Guid id) =>
        ExecuteAsync(async () => { await store.RegisterHabitPositiveAsync(id); await ReloadAsync(); });

    public Task RegisterNegativeAsync(Guid id) =>
        ExecuteAsync(async () => { await store.RegisterHabitNegativeAsync(id); await ReloadAsync(); });

    public Task ToggleTaskAsync(Guid id) =>
        ExecuteAsync(async () => { await store.ToggleTaskAsync(id); await ReloadAsync(); });

    public Task ToggleTodoAsync(Guid id) =>
        ExecuteAsync(async () => { await store.ToggleTodoAsync(id); await ReloadAsync(); });

    public Task ToggleProjectAsync(Guid id) =>
        ExecuteAsync(async () => { await store.ToggleProjectAsync(id); await ReloadAsync(); });

    public Task ReorderHabitsAsync(SortableReorderEvent reorder) =>
        ReorderAsync(ActivityCollection.Habits, FilteredHabits.Select(item => item.Id).ToList(), reorder);

    public Task ReorderTasksAsync(SortableReorderEvent reorder) =>
        ReorderAsync(ActivityCollection.Tasks, FilteredTasks.Select(item => item.Id).ToList(), reorder);

    public Task ReorderTodosAsync(SortableReorderEvent reorder) =>
        ReorderAsync(ActivityCollection.Todos, FilteredTodos.Select(item => item.Id).ToList(), reorder);

    public Task ReorderProjectsAsync(SortableReorderEvent reorder) =>
        ReorderAsync(ActivityCollection.Projects, FilteredProjects.Select(item => item.Id).ToList(), reorder);

    public string FormatRepeat(TaskRepeat repeat) =>
        repeat == TaskRepeat.None ? "No repeat" : repeat.ToString();

    public string FormatDueDate(DateOnly? date) =>
        date?.ToString("MMM dd, yyyy") ?? "No due date";

    public string FormatProjectStatus(ProjectStatus status) => status switch
    {
        ProjectStatus.InProgress => "In progress",
        ProjectStatus.OnHold => "On hold",
        _ => status.ToString()
    };

    private bool MatchesSearch(Activity item) =>
        string.IsNullOrWhiteSpace(search)
        || item.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
        || item.Description.Contains(search, StringComparison.OrdinalIgnoreCase);

    private async Task ReloadAsync() => data = await store.LoadAsync();

    private Task ReorderAsync(
        ActivityCollection collection,
        IReadOnlyList<Guid> currentOrder,
        SortableReorderEvent reorder)
    {
        if (!Guid.TryParse(reorder.ItemId, out var itemId)
            || !Guid.TryParse(reorder.TargetItemId, out var targetItemId))
        {
            return Task.CompletedTask;
        }

        var reorderedIds = SortableOrder.Move(currentOrder, itemId, targetItemId, reorder.PlaceAfter);
        if (reorderedIds.SequenceEqual(currentOrder))
        {
            return Task.CompletedTask;
        }

        return ExecuteAsync(
            async () =>
            {
                await store.ReorderAsync(collection, reorderedIds);
                await ReloadAsync();
            },
            errorMessage: "The new card order could not be saved.");
    }

    private async Task SaveEditorAsync(Func<Task> operation, string successMessage)
    {
        await ExecuteAsync(
            async () =>
            {
                await operation();
                Modals.CloseEditor();
                await ReloadAsync();
            },
            successMessage,
            "Your changes could not be saved.");
    }

    private async Task DeleteCurrentEditorItemAsync(ActivityType expectedType, string successMessage)
    {
        if (Modals.EditingId is not Guid id || Modals.ActiveEditor != expectedType)
        {
            Modals.CloseEditor();
            return;
        }

        await ExecuteAsync(
            async () =>
            {
                await AnimateRemovalAsync(id);
                await DeleteAsync(id, expectedType);
                Modals.CloseEditor();
                await ReloadAsync();
            },
            successMessage,
            "The item could not be deleted.");
    }


    private async Task AnimateRemovalAsync(Guid id)
    {
        RemovingItemId = id;
        Changed?.Invoke();
        await Task.Delay(170);
        RemovingItemId = null;
    }

    private async Task ExecuteAsync(
        Func<Task> operation,
        string? successMessage = null,
        string errorMessage = "The operation could not be completed.")
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await operation();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                toastService.ShowSuccess(successMessage);
            }
        }
        catch
        {
            toastService.ShowError(errorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task DeleteAsync(Guid id, ActivityType type) => type switch
    {
        ActivityType.Habit => store.DeleteHabitAsync(id),
        ActivityType.Task => store.DeleteTaskAsync(id),
        ActivityType.Todo => store.DeleteTodoAsync(id),
        ActivityType.Project => store.DeleteProjectAsync(id),
        _ => Task.CompletedTask
    };
}
