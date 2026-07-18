using LevelUp.Domain.Attributes;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Todos;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Todos;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class ProjectScreen
{
    private readonly ProjectService projects;
    private readonly ProjectTodoService todos;
    private readonly InputReader input;
    private readonly GameStateService state;

    public ProjectScreen(ProjectService projects, ProjectTodoService todos, InputReader input, GameStateService state)
    {
        this.projects = projects;
        this.todos = todos;
        this.input = input;
        this.state = state;
    }

    public void Show()
    {
        while (true)
        {
            ConsoleHelper.ShowHeader("Projects");
            string option = input.ReadSelection("Choose an action:", new[] { "List Projects", "Create Project", "Create To-Do", "Back" }, x => x);
            if (option == "Back") return;
            try
            {
                switch (option)
                {
                    case "List Projects": ListProjects(); break;
                    case "Create Project": CreateProject(); break;
                    case "Create To-Do": CreateTodo(); break;
                }
            }
            catch (Exception ex) { ConsoleHelper.ShowError(ex.Message); }
            input.WaitForContinue();
        }
    }

    private void ListProjects()
    {
        Table table = new Table().AddColumn("Id").AddColumn("Project").AddColumn("Attribute").AddColumn("Status");
        foreach (Project project in projects.GetAllProjects()) table.AddRow(project.Id.ToString(), Markup.Escape(project.Name), project.PrimaryAttribute.ToString(), project.Status.ToString());
        AnsiConsole.Write(table);
    }

    private void CreateProject()
    {
        string name = input.ReadRequiredString("Name:");
        string description = input.ReadRequiredString("Description:");
        AttributeType attribute = input.ReadSelection("Primary attribute:", Enum.GetValues<AttributeType>(), x => x.ToString());
        projects.CreateProject(name, description, attribute);
        state.Save();
        ConsoleHelper.ShowSuccess("Project created successfully.");
    }

    private void CreateTodo()
    {
        Project project = SelectProject();
        string title = input.ReadRequiredString("Title:");
        string description = input.ReadRequiredString("Description:");
        todos.Create(project, title, description);
        state.Save();
        ConsoleHelper.ShowSuccess("To-do created successfully.");
    }

    private Project SelectProject()
    {
        IReadOnlyList<Project> items = projects.GetAllProjects();
        if (items.Count == 0) throw new InvalidOperationException("No projects are available.");
        return input.ReadSelection("Select a project:", items, x => $"#{x.Id} - {x.Name}");
    }


}
