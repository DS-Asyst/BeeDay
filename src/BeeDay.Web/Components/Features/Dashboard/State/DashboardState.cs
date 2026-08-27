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

public sealed class DashboardState(BeeDayWebService store, ToastService toastService, IStringLocalizer<DashboardResources> localizer) : IDisposable
{
    private static readonly UserProfileSummary EmptyProfile = new(
        Guid.Empty, string.Empty, string.Empty, string.Empty, UserLanguage.English, UserTheme.System, 0, 1, 0, 0);

    // Scoped to this circuit's lifetime (DashboardState itself is AddScoped) — cancels every
    // in-flight mutation/query this instance started once the circuit ends, instead of letting
    // abandoned work run to completion server-side with nothing left to observe the result.
    private readonly CancellationTokenSource cancellation = new();

    private DashboardResponse? data;
    private string search = string.Empty;
    private Guid? selectedProjectId;
    private Task? initializationTask;

    public void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }

    public DashboardModalState Modals { get; } = new();
    public DashboardResponse? Data => data;
    public bool IsLoading => data is null;
    public bool IsUnavailable { get; private set; }
    public bool IsBusy { get; private set; }
    public long LatestExperienceGain { get; private set; }
    public long ExperienceFeedbackVersion { get; private set; }
    public Guid? RemovingItemId { get; private set; }
    public event Action? Changed;
    public Task<DashboardResponse> GetDataAsync() => store.LoadDashboardAsync(cancellation.Token);

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

    // Sprint 32.10 (EXP32-F013 follow-up): a single reset for every filter that can narrow the
    // board to zero — search text and the Todos-only project context — so a column's "clear
    // filter" action always recovers fully, regardless of which one caused the empty result.
    public void ClearFilters()
    {
        search = string.Empty;
        selectedProjectId = null;
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

    private bool IsSearchActive => !string.IsNullOrWhiteSpace(search);

    // EXP32-F013 (Sprint 32.8): distinguishes "the search/filter narrowed an active collection to
    // zero" from "this collection genuinely never had any active items" — the two previously
    // rendered the identical empty-state text, so a user searching for an unmatched term could
    // wrongly conclude they had no Tasks/To-Dos/Projects/Habits at all.
    public bool HabitsFilteredToZero =>
        IsSearchActive && !FilteredHabits.Any() && (data?.Habits.Count ?? 0) > 0;

    public bool ActiveTasksFilteredToZero =>
        IsSearchActive && !FilteredTasks.Any(item => !item.Completed) && (data?.Tasks.Count(item => !item.Completed) ?? 0) > 0;

    public bool ActiveTodosFilteredToZero =>
        (IsSearchActive || selectedProjectId is not null)
            && !FilteredTodos.Any(item => !item.Completed)
            && AllTodos.Count(item => !item.Completed) > 0;

    public bool ActiveProjectsFilteredToZero =>
        IsSearchActive && !FilteredProjects.Any(item => !item.Completed) && (data?.Projects.Count(item => !item.Completed) ?? 0) > 0;

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
            () => Modals.EditingId is Guid id ? store.UpdateHabitAsync(id, model, cancellation.Token) : store.AddHabitAsync(model, cancellation.Token),
            Modals.IsEditing ? localizer["HabitUpdatedMessage"] : localizer["HabitCreatedMessage"]);

    public Task SaveTaskAsync(TaskEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateTaskAsync(id, model, cancellation.Token) : store.AddTaskAsync(model, cancellation.Token),
            Modals.IsEditing ? localizer["TaskUpdatedMessage"] : localizer["TaskCreatedMessage"]);

    public Task SaveTodoAsync(TodoEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateTodoAsync(id, model, cancellation.Token) : store.AddTodoAsync(model, cancellation.Token),
            Modals.IsEditing ? localizer["TodoUpdatedMessage"] : localizer["TodoCreatedMessage"]);


    public Task SaveTodoFromProjectAsync(TodoEditorModel model) =>
        ExecuteAsync(
            async () =>
            {
                await store.AddTodoAsync(model, cancellation.Token);
                await ReloadAsync();
            },
            localizer["TodoCreatedMessage"],
            localizer["TodoCreateFromProjectErrorMessage"]);

    public Task SaveProjectAsync(ProjectEditorModel model) =>
        SaveEditorAsync(
            () => Modals.EditingId is Guid id ? store.UpdateProjectAsync(id, model, cancellation.Token) : store.AddProjectAsync(model, cancellation.Token),
            Modals.IsEditing ? localizer["ProjectUpdatedMessage"] : localizer["ProjectCreatedMessage"]);

    public Task DeleteCurrentHabitAsync() => DeleteCurrentEditorItemAsync(ActivityType.Habit, localizer["HabitDeletedMessage"]);
    public Task DeleteCurrentTaskAsync() => DeleteCurrentEditorItemAsync(ActivityType.Task, localizer["TaskDeletedMessage"]);
    public Task DeleteCurrentTodoAsync() => DeleteCurrentEditorItemAsync(ActivityType.Todo, localizer["TodoDeletedMessage"]);
    public Task DeleteCurrentProjectAsync() => DeleteCurrentEditorItemAsync(ActivityType.Project, localizer["ProjectDeletedMessage"]);

    public Task RegisterPositiveAsync(Guid id) =>
        ExecuteExperienceOperationAsync(() => store.RegisterHabitPositiveAsync(id, cancellation.Token));

    public Task RegisterNegativeAsync(Guid id) =>
        ExecuteAsync(async () => { await store.RegisterHabitNegativeAsync(id, cancellation.Token); await ReloadAsync(); });

    public Task ToggleTaskAsync(Guid id) =>
        ExecuteExperienceOperationAsync(() => store.ToggleTaskAsync(id, cancellation.Token));

    public Task ToggleTodoAsync(Guid id) =>
        ExecuteExperienceOperationAsync(() => store.ToggleTodoAsync(id, cancellation.Token));

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
        data = await store.LoadDashboardAsync(cancellation.Token);
        IsUnavailable = false;

        if (selectedProjectId is Guid projectId && !data.Projects.Any(project => project.Id == projectId))
        {
            selectedProjectId = null;
        }

        // EPIC 30 Sprint 30.14: OpenProject already re-derives from data.Projects on every read, so a
        // stale OpenProjectId here has no observable effect — this only keeps the two "current
        // project" ids symmetric, matching selectedProjectId's own reset above.
        if (OpenProjectId is Guid openProjectId && !data.Projects.Any(project => project.Id == openProjectId))
        {
            OpenProjectId = null;
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
                await store.ReorderAsync(collection, reorderedIds, cancellation.Token);
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
                RemovingItemId = id;
                Changed?.Invoke();
                await Task.Delay(170);

                try
                {
                    await DeleteAsync(id, expectedType);
                    Modals.CloseEditor();
                    await ReloadAsync();
                }
                finally
                {
                    // Only clear once the outcome is known: on success the list has already been
                    // reloaded without this item by the time this runs, so there is no flash; on
                    // failure the card reappears exactly when the error toast fires, instead of
                    // snapping back to normal before the delete attempt even completed.
                    RemovingItemId = null;
                }
            },
            successMessage,
            localizer["DeleteErrorMessage"]);
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
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // The circuit is tearing down (Dispose already cancelled this token) — nothing left to
            // show a toast to.
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
        ActivityType.Habit => store.DeleteHabitAsync(id, cancellation.Token),
        ActivityType.Task => store.DeleteTaskAsync(id, cancellation.Token),
        ActivityType.Todo => store.DeleteTodoAsync(id, cancellation.Token),
        ActivityType.Project => store.DeleteProjectAsync(id, cancellation.Token),
        _ => Task.CompletedTask
    };
}
