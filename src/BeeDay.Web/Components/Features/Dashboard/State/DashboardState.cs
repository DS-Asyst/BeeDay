using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Application.Features.Ordering.Requests;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Behaviors.DragDrop;
using BeeDay.Web.Components.Features.Common;
using BeeDay.Web.Components.Features.Habits.Models;
using BeeDay.Web.Components.Features.Projects.Models;
using BeeDay.Web.Components.Features.Tasks.Models;
using BeeDay.Web.Components.Features.Todos.Models;
using BeeDay.Web.Services;

namespace BeeDay.Web.Components.Features.Dashboard.State;

public sealed class DashboardState(BeeDayWebService store, ToastService toastService)
{
    private static readonly UserProfileSummary EmptyProfile = new(
        Guid.Empty, string.Empty, string.Empty, string.Empty, UserLanguage.English, UserTheme.System, 0, 1, 0, 0);

    private DashboardResponse? data;
    private string search = string.Empty;
    private readonly HashSet<ActivityAttribute> selectedAttributes = [];
    private Guid? selectedProjectId;

    public DashboardModalState Modals { get; } = new();
    public DashboardResponse? Data => data;
    public bool IsLoading => data is null;
    public bool IsBusy { get; private set; }
    public long LatestExperienceGain { get; private set; }
    public long ExperienceFeedbackVersion { get; private set; }
    public Guid? RemovingItemId { get; private set; }
    public event Action? Changed;
    public Task<DashboardResponse> GetDataAsync() => store.LoadDashboardAsync();

    public bool HasProfile => data?.Profile.HasProfile == true;
    public Guid? OpenProjectId { get; private set; }
    public ProjectSummary? OpenProject => OpenProjectId is Guid id ? data?.Projects.FirstOrDefault(project => project.Id == id) : null;

    public string Search
    {
        get => search;
        set => search = value ?? string.Empty;
    }

    public IReadOnlyCollection<ActivityAttribute> SelectedAttributes => selectedAttributes;
    public Guid? SelectedProjectId => selectedProjectId;
    public IReadOnlyList<ProjectSummary> ProjectContextOptions => data?.Projects.ToList() ?? [];

    public void SelectProjectContext(Guid? projectId)
    {
        selectedProjectId = projectId is Guid id && data?.Projects.Any(project => project.Id == id) == true
            ? id
            : null;
        Changed?.Invoke();
    }

    public void ToggleAttributeFilter(ActivityAttribute attribute)
    {
        if (!selectedAttributes.Add(attribute))
        {
            selectedAttributes.Remove(attribute);
        }
    }

    public void ClearAttributeFilters() => selectedAttributes.Clear();

    private IEnumerable<TodoSummary> AllTodos => data?.Projects.SelectMany(project => project.Todos) ?? [];

    public int CompletedItems => data is null
        ? 0
        : data.Tasks.Count(item => item.Completed)
          + AllTodos.Count(item => item.Completed)
          + data.Projects.Count(item => item.Completed);

    public int ActiveItems => data is null
        ? 0
        : data.Habits.Count
          + data.Tasks.Count(item => !item.Completed)
          + AllTodos.Count(item => !item.Completed)
          + data.Projects.Count(item => !item.Completed);

    public IEnumerable<HabitSummary> FilteredHabits =>
        Filter(data?.Habits ?? [], item => item.Title, item => item.Description, item => item.Attribute);

    public IEnumerable<TaskSummary> FilteredTasks =>
        Filter(data?.Tasks ?? [], item => item.Title, item => item.Description, item => item.Attribute);

    public IEnumerable<TodoSummary> FilteredTodos =>
        Filter(AllTodos, item => item.Title, item => item.Description, item => item.Attribute)
            .Where(item => selectedProjectId is null || item.ProjectId == selectedProjectId);

    public IEnumerable<ProjectSummary> FilteredProjects =>
        Filter(data?.Projects ?? [], item => item.Name, item => item.Description, item => item.Attribute);

    public int TotalItems => data is null
        ? 0
        : data.Habits.Count + data.Tasks.Count + AllTodos.Count() + data.Projects.Count;

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
            data = new DashboardResponse(EmptyProfile, [], [], [], null);
        }
    }

    public void OpenCreate(ActivityType type) => Modals.OpenCreate(type);
    public void OpenHabitEditor(HabitSummary item) => Modals.OpenHabit(item);
    public void OpenTaskEditor(TaskSummary item) => Modals.OpenTask(item);
    public void OpenTodoEditor(TodoSummary item) => Modals.OpenTodo(item);
    public void OpenProjectEditor(ProjectSummary item) => Modals.OpenProject(item);
    public void OpenProjectWorkspace(ProjectSummary item) { OpenProjectId = item.Id; Changed?.Invoke(); }
    public void OpenProjectFromEditor()
    {
        if (Modals.EditingId is Guid id && data?.Projects.FirstOrDefault(project => project.Id == id) is ProjectSummary project)
        {
            Modals.CloseEditor();
            OpenProjectWorkspace(project);
        }
    }
    public void CloseProjectWorkspace() { OpenProjectId = null; Changed?.Invoke(); }
    public void OpenTodoForProject()
    {
        if (OpenProjectId is Guid projectId)
        {
            Modals.OpenTodoForProject(projectId);
            Changed?.Invoke();
        }
    }
    public void CloseEditor() => Modals.CloseEditor();

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


    public Task SaveTodoFromProjectAsync(TodoEditorModel model) =>
        ExecuteAsync(
            async () =>
            {
                await store.AddTodoAsync(model);
                await ReloadAsync();
            },
            "To-Do created successfully.",
            "The To-Do could not be created.");

    public Task SaveProjectAsync(ProjectEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateProjectAsync(id, model) : store.AddProjectAsync(model),
            Modals.IsEditing ? "Project updated successfully." : "Project created successfully.");

    public Task DeleteCurrentHabitAsync() => DeleteCurrentEditorItemAsync(ActivityType.Habit, "Habit deleted successfully.");
    public Task DeleteCurrentTaskAsync() => DeleteCurrentEditorItemAsync(ActivityType.Task, "Task deleted successfully.");
    public Task DeleteCurrentTodoAsync() => DeleteCurrentEditorItemAsync(ActivityType.Todo, "To-Do deleted successfully.");
    public Task DeleteCurrentProjectAsync() => DeleteCurrentEditorItemAsync(ActivityType.Project, "Project deleted successfully.");

    public Task RegisterPositiveAsync(Guid id) =>
        ExecuteExperienceOperationAsync(() => store.RegisterHabitPositiveAsync(id));

    public Task RegisterNegativeAsync(Guid id) =>
        ExecuteAsync(async () => { await store.RegisterHabitNegativeAsync(id); await ReloadAsync(); });

    public Task ToggleTaskAsync(Guid id) =>
        ExecuteExperienceOperationAsync(() => store.ToggleTaskAsync(id));

    public Task ToggleTodoAsync(Guid id) =>
        ExecuteExperienceOperationAsync(() => store.ToggleTodoAsync(id));

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
        _ => status.ToString()
    };

    // Private helper, not a public Application-layer abstraction: the four summary DTOs
    // (HabitSummary/TaskSummary/TodoSummary/ProjectSummary) deliberately share no interface — this
    // just extracts the three fields each caller already has, via delegates supplied at each call
    // site, so the search/attribute predicate isn't duplicated four times.
    private IEnumerable<T> Filter<T>(
        IEnumerable<T> items,
        Func<T, string> title,
        Func<T, string> description,
        Func<T, ActivityAttribute?> attribute) =>
        items.Where(item => MatchesFilters(title(item), description(item), attribute(item)));

    private bool MatchesFilters(string title, string description, ActivityAttribute? attribute) =>
        (selectedAttributes.Count == 0
            || attribute is ActivityAttribute value && selectedAttributes.Contains(value))
        && (string.IsNullOrWhiteSpace(search)
            || title.Contains(search, StringComparison.OrdinalIgnoreCase)
            || description.Contains(search, StringComparison.OrdinalIgnoreCase)
            || (attribute?.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

    private async Task ReloadAsync()
    {
        data = await store.LoadDashboardAsync();

        if (selectedProjectId is Guid projectId && !data.Projects.Any(project => project.Id == projectId))
        {
            selectedProjectId = null;
        }
    }

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

    private async Task ExecuteExperienceOperationAsync(Func<Task> operation)
    {
        var previousExperience = data?.Profile.TotalExperience ?? 0;

        await ExecuteAsync(async () =>
        {
            await operation();
            await ReloadAsync();
            ShowExperienceGain(previousExperience);
        });
    }

    private void ShowExperienceGain(long previousExperience)
    {
        var currentExperience = data?.Profile.TotalExperience ?? previousExperience;
        var gainedExperience = currentExperience - previousExperience;

        if (gainedExperience <= 0)
        {
            return;
        }

        LatestExperienceGain = gainedExperience;
        ExperienceFeedbackVersion++;
        Changed?.Invoke();
        _ = ClearExperienceFeedbackAsync(ExperienceFeedbackVersion);
    }

    private async Task ClearExperienceFeedbackAsync(long feedbackVersion)
    {
        await Task.Delay(750);

        if (ExperienceFeedbackVersion != feedbackVersion)
        {
            return;
        }

        LatestExperienceGain = 0;
        Changed?.Invoke();
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
