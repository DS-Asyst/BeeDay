using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using Spectre.Console;


namespace LevelUp.UI;

public sealed class ProjectScreen
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly InputReader inputReader;
    private readonly GameStateService gameStateService;


    public ProjectScreen(
        ProjectService projectService,
        QuestService questService,
        InputReader inputReader,
        GameStateService gameStateService
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.inputReader = inputReader;
        this.gameStateService = gameStateService;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Projects");

            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "Create project",
                    "View projects",
                    "Activate project",
                    "Edit project",
                    "Archive project",
                    "Delete project",
                    "Back"
                },
                choice => choice
            );

            running = HandleOption(option);
        }
    }

    private bool HandleOption(string option)
    {
        switch (option)
        {
            case "Create project":
                CreateProject();
                inputReader.WaitForContinue();
                return true;

            case "View projects":
                ListProjects();
                inputReader.WaitForContinue();
                return true;

            case "Activate project":
                ActivateProject();
                inputReader.WaitForContinue();
                return true;

            case "Edit project":
                EditProject();
                inputReader.WaitForContinue();
                return true;

            case "Archive project":
                ArchiveProject();
                inputReader.WaitForContinue();
                return true;

            case "Delete project":
                DeleteProject();
                inputReader.WaitForContinue();
                return true;

            case "Back":
                return false;

            default:
                return true;
        }
    }

    private void CreateProject()
    {
        ConsoleHelper.ShowHeader("Create Project");

        string name = inputReader.ReadRequiredString(
            "Project name:"
        );

        string description = inputReader.ReadRequiredString(
            "Description:"
        );

        string unlockedTitle = inputReader.ReadRequiredString(
            "Unlocked title:"
        );

        Project project = projectService.CreateProject(
            name,
            description,
            unlockedTitle
        );

        gameStateService.Save();

        ConsoleHelper.ShowSuccess(
            "Project created successfully."
        );

        ShowProjectDetails(project);
    }

    private void ListProjects()
    {
        ConsoleHelper.ShowHeader("Projects");

        IReadOnlyList<Project> projects =
            projectService.GetAllProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No projects registered."
            );

            return;
        }

        Table table = new();

        table.AddColumn("ID");
        table.AddColumn("Name");
        table.AddColumn("Status");
        table.AddColumn("Quests");
        table.AddColumn("Progress");
        table.AddColumn("Unlocked Title");

        foreach (Project project in projects)
        {
            IReadOnlyList<Quest> projectQuests =
                questService.GetQuestsByProjectId(
                    project.Id
                );

            int completedQuests = projectQuests.Count(
                quest =>
                    quest.Status == QuestStatus.Completed
            );

            decimal progress =
                projectService.CalculateProgress(
                    project,
                    questService.GetAllQuests()
                );

            table.AddRow(
                project.Id.ToString(),
                Markup.Escape(project.Name),
                project.Status.ToString(),
                $"{completedQuests}/{projectQuests.Count}",
                $"{progress:0.##}%",
                Markup.Escape(project.UnlockedTitle)
            );
        }

        AnsiConsole.Write(table);
    }

    private void ActivateProject()
    {
        ConsoleHelper.ShowHeader("Activate Project");

        List<Project> availableProjects = projectService
            .GetAllProjects()
            .Where(project =>
                project.Status == ProjectStatus.Created
            )
            .ToList();

        if (availableProjects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "There are no created projects to activate."
            );

            return;
        }

        Project selectedProject = SelectProject(
            "Select a project to activate:",
            availableProjects
        );

        bool confirmed = inputReader.ReadConfirmation(
            $"Activate '{selectedProject.Name}'?"
        );

        if (!confirmed)
        {
            ConsoleHelper.ShowInformation(
                "Project activation cancelled."
            );

            return;
        }

        projectService.ActivateProject(
            selectedProject
        );

        gameStateService.Save();

        ConsoleHelper.ShowSuccess(
            "Project activated successfully."
        );
    }

    private void EditProject()
    {
        ConsoleHelper.ShowHeader("Edit Project");

        List<Project> availableProjects = projectService
            .GetAllProjects()
            .Where(project =>
                project.Status != ProjectStatus.Archived
            )
            .ToList();

        if (availableProjects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "There are no projects available to edit."
            );

            return;
        }

        Project selectedProject = SelectProject(
            "Select a project to edit:",
            availableProjects
        );

        AnsiConsole.MarkupLine(
            $"[grey]Current name:[/] " +
            $"{Markup.Escape(selectedProject.Name)}"
        );

        string name = inputReader.ReadRequiredString(
            "New project name:"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Current description:[/] " +
            $"{Markup.Escape(selectedProject.Description)}"
        );

        string description = inputReader.ReadRequiredString(
            "New description:"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Current unlocked title:[/] " +
            $"{Markup.Escape(selectedProject.UnlockedTitle)}"
        );

        string unlockedTitle =
            inputReader.ReadRequiredString(
                "New unlocked title:"
            );

        bool confirmed = inputReader.ReadConfirmation(
            $"Save changes to '{selectedProject.Name}'?"
        );

        if (!confirmed)
        {
            ConsoleHelper.ShowInformation(
                "Project update cancelled."
            );

            return;
        }

        projectService.UpdateProject(
            selectedProject,
            name,
            description,
            unlockedTitle
        );

        gameStateService.Save();

        ConsoleHelper.ShowSuccess(
            "Project updated successfully."
        );
    }

    private void ArchiveProject()
    {
        ConsoleHelper.ShowHeader("Archive Project");

        List<Project> availableProjects = projectService
            .GetAllProjects()
            .Where(project =>
                project.Status != ProjectStatus.Archived
            )
            .ToList();

        if (availableProjects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "There are no projects available to archive."
            );

            return;
        }

        Project selectedProject = SelectProject(
            "Select a project to archive:",
            availableProjects
        );

        bool confirmed = inputReader.ReadConfirmation(
            $"Archive '{selectedProject.Name}'?"
        );

        if (!confirmed)
        {
            ConsoleHelper.ShowInformation(
                "Project archive cancelled."
            );

            return;
        }

        projectService.ArchiveProject(
            selectedProject
        );

        gameStateService.Save();

        ConsoleHelper.ShowSuccess(
            "Project archived successfully."
        );
    }

    private void DeleteProject()
    {
        ConsoleHelper.ShowHeader("Delete Project");

        IReadOnlyList<Project> projects =
            projectService.GetAllProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "There are no projects available to delete."
            );

            return;
        }

        Project selectedProject = SelectProject(
            "Select a project to delete:",
            projects
        );

        IReadOnlyList<Quest> linkedQuests =
            questService.GetQuestsByProjectId(
                selectedProject.Id
            );

        if (linkedQuests.Count > 0)
        {
            ConsoleHelper.ShowInformation(
                $"This project has {linkedQuests.Count} linked quest(s)."
            );

            bool removeAssociations =
                inputReader.ReadConfirmation(
                    "Remove the project association from these quests?"
                );

            if (!removeAssociations)
            {
                ConsoleHelper.ShowInformation(
                    "Project deletion cancelled."
                );

                return;
            }

            foreach (Quest quest in linkedQuests)
            {
                questService.RemoveQuestFromProject(
                    quest
                );
            }
        }

        bool confirmed = inputReader.ReadConfirmation(
            $"Permanently delete '{selectedProject.Name}'?"
        );

        if (!confirmed)
        {
            ConsoleHelper.ShowInformation(
                "Project deletion cancelled."
            );

            return;
        }

        bool deleted = projectService.DeleteProject(
            selectedProject.Id
        );

        if (!deleted)
        {
            ConsoleHelper.ShowInformation(
                "The project could not be deleted."
            );

            return;
        }

        gameStateService.Save();

        ConsoleHelper.ShowSuccess(
            "Project deleted successfully."
        );
    }

    private Project SelectProject(
        string prompt,
        IEnumerable<Project> projects
    )
    {
        return inputReader.ReadSelection(
            prompt,
            projects,
            project =>
                $"{project.Name} — {project.Status}"
        );
    }

    private void ShowProjectDetails(Project project)
    {
        AnsiConsole.MarkupLine(
            $"[grey]ID:[/] {project.Id}"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Name:[/] " +
            $"{Markup.Escape(project.Name)}"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Status:[/] {project.Status}"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Unlocked title:[/] " +
            $"{Markup.Escape(project.UnlockedTitle)}"
        );
    }

}