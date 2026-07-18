using LevelUp.Domain.Todos;
using LevelUp.Services.Persistence;
using LevelUp.Services.Todos;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class TodoScreen
{
    private readonly ProjectTodoService todos;
    private readonly InputReader input;
    private readonly GameStateService state;

    public TodoScreen(ProjectTodoService todos, InputReader input, GameStateService state)
    {
        this.todos = todos;
        this.input = input;
        this.state = state;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.ShowHeader("To-Dos");
            string option = input.ReadSelection(
                "Choose an action:",
                new[] { "List To-Dos", "Open To-Do", "Activate To-Do", "Complete To-Do", "Delete To-Do", "Back" },
                choice => choice);

            if (option == "Back") return;

            try
            {
                switch (option)
                {
                    case "List To-Dos": ListTodos(); break;
                    case "Open To-Do": OpenTodo(); break;
                    case "Activate To-Do": ActivateTodo(); break;
                    case "Complete To-Do": CompleteTodo(); break;
                    case "Delete To-Do": DeleteTodo(); break;
                }
            }
            catch (Exception exception)
            {
                ConsoleHelper.ShowError(exception.Message);
            }

            input.WaitForContinue();
        }
    }

    private void ListTodos()
    {
        IReadOnlyList<ProjectTodo> items = todos.GetAll();
        if (items.Count == 0)
        {
            ConsoleHelper.ShowInformation("No to-dos are available.");
            return;
        }

        Table table = new Table()
            .AddColumn("Id")
            .AddColumn("To-Do")
            .AddColumn("Project Id")
            .AddColumn("Milestone Id")
            .AddColumn("Status");

        foreach (ProjectTodo todo in items)
        {
            table.AddRow(
                todo.Id.ToString(),
                Markup.Escape(todo.Title),
                todo.ProjectId.ToString(),
                todo.MilestoneId?.ToString() ?? "—",
                todo.Status.ToString());
        }

        AnsiConsole.Write(table);
    }

    private void OpenTodo()
    {
        ProjectTodo todo = SelectTodo();
        ConsoleHelper.ShowHeader("To-Do");
        AnsiConsole.MarkupLine($"[grey]Title:[/] {Markup.Escape(todo.Title)}");
        AnsiConsole.MarkupLine($"[grey]Description:[/] {Markup.Escape(todo.Description)}");
        AnsiConsole.MarkupLine($"[grey]Project Id:[/] {todo.ProjectId}");
        AnsiConsole.MarkupLine($"[grey]Milestone Id:[/] {todo.MilestoneId?.ToString() ?? "—"}");
        AnsiConsole.MarkupLine($"[grey]Status:[/] {todo.Status}");
    }

    private void ActivateTodo()
    {
        ProjectTodo todo = SelectTodo();
        todos.Activate(todo);
        state.Save();
        ConsoleHelper.ShowSuccess("To-do activated successfully.");
    }

    private void CompleteTodo()
    {
        ProjectTodo todo = SelectTodo();
        todos.Complete(todo);
        state.Save();
        ConsoleHelper.ShowSuccess("To-do completed successfully.");
    }

    private void DeleteTodo()
    {
        ProjectTodo todo = SelectTodo();
        todos.Delete(todo.Id);
        state.Save();
        ConsoleHelper.ShowSuccess("To-do deleted successfully.");
    }

    private ProjectTodo SelectTodo()
    {
        IReadOnlyList<ProjectTodo> items = todos.GetAll();
        if (items.Count == 0) throw new InvalidOperationException("No to-dos are available.");
        return input.ReadSelection("Select a to-do:", items, todo => $"#{todo.Id} - {todo.Title} ({todo.Status})");
    }
}
