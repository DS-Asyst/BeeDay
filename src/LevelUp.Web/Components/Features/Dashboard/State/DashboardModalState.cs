using LevelUp.Domain.Entities;
using LevelUp.Web.Components.Features.Common;
using LevelUp.Web.Components.Features.Habits.Models;
using LevelUp.Web.Components.Features.Projects.Models;
using LevelUp.Web.Components.Features.Tasks.Models;
using LevelUp.Web.Components.Features.Todos.Models;

namespace LevelUp.Web.Components.Features.Dashboard.State;

public sealed class DashboardModalState
{
    public Guid? EditingId { get; private set; }
    public ActivityType? ActiveEditor { get; private set; }
    public HabitEditorModel HabitForm { get; private set; } = new();
    public TaskEditorModel TaskForm { get; private set; } = new();
    public TodoEditorModel TodoForm { get; private set; } = new();
    public ProjectEditorModel ProjectForm { get; private set; } = new();

    public bool IsDeleteConfirmationOpen { get; private set; }
    public Guid? PendingDeleteId { get; private set; }
    public ActivityType? PendingDeleteType { get; private set; }
    public string PendingDeleteTitle { get; private set; } = string.Empty;

    public bool IsEditing => EditingId is not null;
    public bool IsHabitEditorOpen => ActiveEditor == ActivityType.Habit;
    public bool IsTaskEditorOpen => ActiveEditor == ActivityType.Task;
    public bool IsTodoEditorOpen => ActiveEditor == ActivityType.Todo;
    public bool IsProjectEditorOpen => ActiveEditor == ActivityType.Project;

    public string DeleteItemDisplayName => PendingDeleteType switch
    {
        ActivityType.Habit => "Habit",
        ActivityType.Todo => "To-Do",
        ActivityType.Project => "Project",
        _ => "Task"
    };

    public void OpenCreate(ActivityType type)
    {
        EditingId = null;
        ActiveEditor = type;

        switch (type)
        {
            case ActivityType.Habit:
                HabitForm = new HabitEditorModel();
                break;
            case ActivityType.Task:
                TaskForm = new TaskEditorModel();
                break;
            case ActivityType.Todo:
                TodoForm = new TodoEditorModel();
                break;
            case ActivityType.Project:
                ProjectForm = new ProjectEditorModel();
                break;
        }
    }


    public void OpenTodoForProject(Guid projectId)
    {
        EditingId = null;
        TodoForm = new TodoEditorModel { ProjectId = projectId };
        ActiveEditor = ActivityType.Todo;
    }

    public void OpenHabit(Habit item)
    {
        EditingId = item.Id;
        HabitForm = new HabitEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Direction = item.Direction,
            Difficulty = item.Difficulty,
            ResetCounter = item.ResetCounter
        };
        ActiveEditor = ActivityType.Habit;
    }

    public void OpenTask(RecurringTask item)
    {
        EditingId = item.Id;
        TaskForm = new TaskEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Repeat = item.Repeat
        };
        ActiveEditor = ActivityType.Task;
    }

    public void OpenTodo(Todo item)
    {
        EditingId = item.Id;
        TodoForm = new TodoEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate?.ToDateTime(TimeOnly.MinValue),
            ProjectId = item.ProjectId
        };
        ActiveEditor = ActivityType.Todo;
    }

    public void OpenProject(Project item)
    {
        EditingId = item.Id;
        ProjectForm = new ProjectEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Color = item.Color,
            ExpectedDate = item.ExpectedDate?.ToDateTime(TimeOnly.MinValue),
            Archived = item.Archived
        };
        ActiveEditor = ActivityType.Project;
    }

    public void CloseEditor()
    {
        ActiveEditor = null;
        EditingId = null;
    }

    public void RequestDelete(Guid id, ActivityType type, string title)
    {
        PendingDeleteId = id;
        PendingDeleteType = type;
        PendingDeleteTitle = title;
        IsDeleteConfirmationOpen = true;
    }

    public void CancelDelete()
    {
        IsDeleteConfirmationOpen = false;
        PendingDeleteId = null;
        PendingDeleteType = null;
        PendingDeleteTitle = string.Empty;
    }
}
