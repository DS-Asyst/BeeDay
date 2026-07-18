using LevelUp.Domain.Attributes;
using LevelUp.Domain.Tasks;
using LevelUp.Services.Persistence;
using LevelUp.Services.Tasks;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class TaskScreen
{
    private readonly TaskService tasks;
    private readonly GameStateService state;
    private readonly InputReader input;

    public TaskScreen(TaskService tasks, GameStateService state, InputReader input)
    {
        this.tasks = tasks;
        this.state = state;
        this.input = input;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.ShowHeader("Tasks");
            string option = input.ReadSelection("Choose an action:", new[] { "List", "Create", "Complete", "Delete", "Back" }, x => x);
            if (option == "Back") return;
            try
            {
                switch (option)
                {
                    case "List": List(); break;
                    case "Create": Create(); break;
                    case "Complete": Complete(); break;
                    case "Delete": Delete(); break;
                }
            }
            catch (Exception ex) { ConsoleHelper.ShowError(ex.Message); }
            input.WaitForContinue();
        }
    }

    private void List()
    {
        Table table = new Table().AddColumn("Id").AddColumn("Task").AddColumn("Recurrence").AddColumn("Status").AddColumn("Completions");
        foreach (TaskItem task in tasks.GetAll())
            table.AddRow(task.Id.ToString(), Markup.Escape(task.Title), task.Recurrence.ToString(), task.Status.ToString(), task.CompletionCount.ToString());
        AnsiConsole.Write(table);
    }

    private void Create()
    {
        string title = input.ReadRequiredString("Title:");
        string description = input.ReadRequiredString("Description:");
        AttributeType attribute = input.ReadSelection("Attribute:", Enum.GetValues<AttributeType>(), x => x.ToString());
        TaskRecurrence recurrence = input.ReadSelection("Recurrence:", Enum.GetValues<TaskRecurrence>(), x => x.ToString());
        WeekDays days = recurrence == TaskRecurrence.Daily
            ? WeekDays.EveryDay
            : input.ReadSelection("Weekday:", new[] { WeekDays.Monday, WeekDays.Tuesday, WeekDays.Wednesday, WeekDays.Thursday, WeekDays.Friday, WeekDays.Saturday, WeekDays.Sunday }, x => x.ToString());
        tasks.Create(title, description, attribute, recurrence, days);
        state.Save();
        ConsoleHelper.ShowSuccess("Task created successfully.");
    }

    private void Complete()
    {
        TaskItem task = Select();
        tasks.Complete(task);
        state.Save();
        ConsoleHelper.ShowSuccess("Task completed successfully.");
    }

    private void Delete()
    {
        TaskItem task = Select();
        tasks.Delete(task.Id);
        state.Save();
        ConsoleHelper.ShowSuccess("Task deleted successfully.");
    }

    private TaskItem Select()
    {
        IReadOnlyList<TaskItem> items = tasks.GetAll();
        if (items.Count == 0) throw new InvalidOperationException("No tasks are available.");
        return input.ReadSelection("Select a task:", items, x => $"#{x.Id} - {x.Title}");
    }
}
