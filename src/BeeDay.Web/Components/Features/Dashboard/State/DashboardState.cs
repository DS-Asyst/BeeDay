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
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.Dashboard.State;

public sealed class DashboardState(BeeDayWebService store, ToastService toastService, IStringLocalizer<DashboardResources> localizer)
{
    private static readonly UserProfileSummary EmptyProfile = new(
        Guid.Empty, string.Empty, string.Empty, string.Empty, UserLanguage.English, UserTheme.System, 0, 1, 0, 0);

    private DashboardResponse? data;
    private string search = string.Empty;
    private Guid? selectedProjectId;
    private Task? initializationTask;

    public DashboardModalState Modals { get; } = new();
    public DashboardResponse? Data => data;
    public bool IsLoading => data is null;
    public bool IsUnavailable { get; private set; }
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

    public Guid? SelectedProjectId => selectedProjectId;
    public IReadOnlyList<ProjectSummary> ProjectContextOptions => data?.Projects.ToList() ?? [];

    public void SelectProjectContext(Guid? projectId)
    {
        selectedProjectId = projectId is Guid id && data?.Projects.Any(project => project.Id == id) == true
            ? id
            : null;
        Changed?.Invoke();
    }

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
        Filter(data?.Habits ?? [], item => item.Title, item => item.Description);

    public IEnumerable<TaskSummary> FilteredTasks =>
        Filter(data?.Tasks ?? [], item => item.Title, item => item.Description);

    public IEnumerable<TodoSummary> FilteredTodos =>
        Filter(AllTodos, item => item.Title, item => item.Description)
            .Where(item => selectedProjectId is null || item.ProjectId == selectedProjectId);

    public IEnumerable<ProjectSummary> FilteredProjects =>
        Filter(data?.Projects ?? [], item => item.Name, item => item.Description);

    public int TotalItems => data is null
        ? 0
        : data.Habits.Count + data.Tasks.Count + AllTodos.Count() + data.Projects.Count;

    public int FilteredItems => FilteredHabits.Count() + FilteredTasks.Count() + FilteredTodos.Count() + FilteredProjects.Count();

    public Task InitializeAsync() => initializationTask ??= InitializeCoreAsync();

    private async Task InitializeCoreAsync()
    {
        try
        {
            await ReloadAsync();
        }
        catch
        {
            toastService.ShowError(localizer["DashboardLoadErrorMessage"]);
            data = new DashboardResponse(EmptyProfile, [], [], [], null);
            IsUnavailable = true;
            Changed?.Invoke();
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
            Modals.IsEditing ? localizer["HabitUpdatedMessage"] : localizer["HabitCreatedMessage"]);

    public Task SaveTaskAsync(TaskEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateTaskAsync(id, model) : store.AddTaskAsync(model),
            Modals.IsEditing ? localizer["TaskUpdatedMessage"] : localizer["TaskCreatedMessage"]);

    public Task SaveTodoAsync(TodoEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateTodoAsync(id, model) : store.AddTodoAsync(model),
            Modals.IsEditing ? localizer["TodoUpdatedMessage"] : localizer["TodoCreatedMessage"]);


    public Task SaveTodoFromProjectAsync(TodoEditorModel model) =>
        ExecuteAsync(
            async () =>
            {
                await store.AddTodoAsync(model);
                await ReloadAsync();
            },
            localizer["TodoCreatedMessage"],
            localizer["TodoCreateFromProjectErrorMessage"]);

    public Task SaveProjectAsync(ProjectEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateProjectAsync(id, model) : store.AddProjectAsync(model),
            Modals.IsEditing ? localizer["ProjectUpdatedMessage"] : localizer["ProjectCreatedMessage"]);

    public Task DeleteCurrentHabitAsync() => DeleteCurrentEditorItemAsync(ActivityType.Habit, localizer["HabitDeletedMessage"]);
    public Task DeleteCurrentTaskAsync() => DeleteCurrentEditorItemAsync(ActivityType.Task, localizer["TaskDeletedMessage"]);
    public Task DeleteCurrentTodoAsync() => DeleteCurrentEditorItemAsync(ActivityType.Todo, localizer["TodoDeletedMessage"]);
    public Task DeleteCurrentProjectAsync() => DeleteCurrentEditorItemAsync(ActivityType.Project, localizer["ProjectDeletedMessage"]);

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

    public string FormatRepeat(TaskRepeat repeat) => repeat switch
    {
        TaskRepeat.None => localizer["NoRepeatLabel"],
        TaskRepeat.Daily => localizer["TaskRepeatDaily"],
        TaskRepeat.Weekly => localizer["TaskRepeatWeekly"],
        TaskRepeat.Monthly => localizer["TaskRepeatMonthly"],
        _ => repeat.ToString()
    };

    public string FormatDueDate(DateOnly? date) =>
        date?.ToString("d") ?? localizer["NoDueDateLabel"];

    public string FormatProjectStatus(ProjectStatus status) => status switch
    {
        ProjectStatus.Planned => localizer["ProjectStatusPlanned"],
        ProjectStatus.InProgress => localizer["ProjectInProgressLabel"],
        ProjectStatus.Completed => localizer["ProjectStatusCompleted"],
        _ => status.ToString()
    };

    private IEnumerable<T> Filter<T>(
        IEnumerable<T> items,
        Func<T, string> title,
        Func<T, string> description) =>
        items.Where(item => MatchesSearch(title(item), description(item)));

    private bool MatchesSearch(string title, string description) =>
        string.IsNullOrWhiteSpace(search)
            || title.Contains(search, StringComparison.OrdinalIgnoreCase)
            || description.Contains(search, StringComparison.OrdinalIgnoreCase);

    private async Task ReloadAsync()
    {
        data = await store.LoadDashboardAsync();
        IsUnavailable = false;

        if (selectedProjectId is Guid projectId && !data.Projects.Any(project => project.Id == projectId))
        {
            selectedProjectId = null;
        }

        Changed?.Invoke();
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
            errorMessage: localizer["ReorderErrorMessage"]);
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
            localizer["SaveErrorMessage"]);
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
            localizer["DeleteErrorMessage"]);
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
        string? errorMessage = null)
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
            toastService.ShowError(errorMessage ?? localizer["GenericOperationErrorMessage"]);
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
