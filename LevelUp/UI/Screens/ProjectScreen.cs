using LevelUp.Domain.Projects;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class ProjectScreen
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;

    public ProjectScreen(
        ProjectService projectService,
        QuestService questService
    )
    {
        this.projectService = projectService;
        this.questService = questService;
    }

    public void Show()
    {
        AnsiConsole.Clear();

        IReadOnlyList<Project> projects =
            projectService.GetAllProjects();

        AnsiConsole.Write(
            new Rule("[bold green]Projects[/]")
        );

        if (projects.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]No projects registered.[/]"
            );

            AnsiConsole.MarkupLine(
                "\n[grey]Press any key to return.[/]"
            );

            Console.ReadKey(true);

            return;
        }

        Table table = new();

        table.AddColumn("ID");
        table.AddColumn("Name");
        table.AddColumn("Status");
        table.AddColumn("Progress");

        foreach (Project project in projects)
        {
            decimal progress =
                projectService.CalculateProgress(
                    project,
                    questService.GetAllQuests()
                );

            table.AddRow(
                project.Id.ToString(),
                Markup.Escape(project.Name),
                project.Status.ToString(),
                $"{progress:0.##}%"
            );
        }

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine(
            "\n[grey]Press any key to return.[/]"
        );

        Console.ReadKey(true);
    }
}