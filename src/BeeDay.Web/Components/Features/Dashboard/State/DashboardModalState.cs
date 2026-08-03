using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Web.Components.Features.Common;
using BeeDay.Web.Components.Features.Habits.Models;
using BeeDay.Web.Components.Features.Projects.Models;
using BeeDay.Web.Components.Features.Tasks.Models;
using BeeDay.Web.Components.Features.Todos.Models;

namespace BeeDay.Web.Components.Features.Dashboard.State;

public sealed class DashboardModalState
{
    public Guid? EditingId { get; private set; }
    public ActivityType? ActiveEditor { get; private set; }
    public HabitEditorModel HabitForm { get; private set; } = new();
    public TaskEditorModel TaskForm { get; private set; } = new();
    public TodoEditorModel TodoForm { get; private set; } = new();
    public ProjectEditorModel ProjectForm { get; private set; } = new();

    public bool IsEditing => EditingId is not null;
    public bool IsHabitEditorOpen => ActiveEditor == ActivityType.Habit;
    public bool IsTaskEditorOpen => ActiveEditor == ActivityType.Task;
    public bool IsTodoEditorOpen => ActiveEditor == ActivityType.Todo;
    public bool IsProjectEditorOpen => ActiveEditor == ActivityType.Project;

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

    public void OpenHabit(HabitSummary item)
    {
        EditingId = item.Id;
        HabitForm = new HabitEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Direction = item.Direction,
            Difficulty = item.Difficulty,
            ResetCounter = item.ResetCounter,
            Attribute = item.Attribute,
            VisualBalance = item.PositiveCount - item.NegativeCount
        };
        ActiveEditor = ActivityType.Habit;
    }

    public void OpenTask(TaskSummary item)
    {
        EditingId = item.Id;
        TaskForm = new TaskEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Repeat = item.Repeat,
            Attribute = item.Attribute
        };
        ActiveEditor = ActivityType.Task;
    }

    public void OpenTodo(TodoSummary item)
    {
        EditingId = item.Id;
        TodoForm = new TodoEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate?.ToDateTime(TimeOnly.MinValue),
            ProjectId = item.ProjectId,
            Attribute = item.Attribute
        };
        ActiveEditor = ActivityType.Todo;
    }

    public void OpenProject(ProjectSummary item)
    {
        EditingId = item.Id;
        ProjectForm = new ProjectEditorModel
        {
            Title = item.Name,
            Description = item.Description,
            ExpectedDate = item.ExpectedDate?.ToDateTime(TimeOnly.MinValue),
            Archived = item.Archived,
            Attribute = item.Attribute
        };
        ActiveEditor = ActivityType.Project;
    }

    public void CloseEditor()
    {
        ActiveEditor = null;
        EditingId = null;
    }

}
