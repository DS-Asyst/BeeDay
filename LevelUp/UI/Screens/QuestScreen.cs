using LevelUp.Domain.Milestones;
using LevelUp.Domain.Attributes;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Quest;
using Spectre.Console;
using QuestModel = LevelUp.Domain.Quests.Quest;
using LevelUp.UI.Infrastructure;
using LevelUp.UI.Flows.Quests;

namespace LevelUp.UI;

public sealed class QuestScreen
{
    private readonly QuestService questService;
    private readonly ProjectService projectService;
    private readonly InputReader inputReader;
    private readonly GameStateService gameStateService;
    private readonly QuestWorkflowService questWorkflowService;
    private readonly QuestSelectionFlow selectionFlow;

    public QuestScreen(
        QuestService questService,
        ProjectService projectService,
        InputReader inputReader,
        GameStateService gameStateService,
        QuestWorkflowService questWorkflowService,
        QuestSelectionFlow selectionFlow
    )
    {
        this.questService = questService;
        this.projectService = projectService;
        this.inputReader = inputReader;
        this.gameStateService = gameStateService;
        this.questWorkflowService = questWorkflowService;
        this.selectionFlow = selectionFlow;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Task Board");

            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "New Task",
                    "Open Task",
                    "List Tasks",
                    "Back"
                },
                choice => choice
            );

            switch (option)
            {
                case "New Task":
                    CreateQuest();
                    inputReader.WaitForContinue();
                    break;

                case "Open Task":
                    OpenQuest();
                    break;

                case "List Tasks":
                    ListQuests();
                    inputReader.WaitForContinue();
                    break;

                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private void CreateQuest()
    {
        ConsoleHelper.ShowHeader("New Task");
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel("Title:");
            string description = inputReader.ReadRequiredStringOrCancel(
                "Description:"
            );
            Project? project = selectionFlow.SelectOptionalProjectForCreation();
            Milestone? milestone = project is null
                ? null
                : selectionFlow.SelectOptionalMilestoneForCreation(project);
            AttributeType independentAttribute = project is null
                ? inputReader.ReadSelection(
                    "Task attribute:",
                    Enum.GetValues<AttributeType>(),
                    DisplayText.For
                )
                : project.PrimaryAttribute;

            Quest quest = questService.CreateQuest(
                title,
                description,
                project,
                independentAttribute
            );

            if (milestone is not null)
            {
                questService.AssignQuestToMilestone(quest, milestone);
            }

            questService.ActivateQuest(quest);
            gameStateService.Save();

            ConsoleHelper.ShowSuccess(
                "Task created successfully."
            );
            AnsiConsole.WriteLine();
            AnsiConsole.Write(BuildQuestCard(quest).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation(
                "Task creation cancelled."
            );
        }
    }

    private void OpenQuest()
    {
        IReadOnlyList<QuestModel> quests =
            questService.GetAllQuests();

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No tasks have been created."
            );
            inputReader.WaitForContinue();
            return;
        }

        Quest selectedQuest = selectionFlow.SelectQuest(
            "Select a task:",
            quests
        );

        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Task");
            AnsiConsole.Write(
                BuildQuestCard(selectedQuest).Build()
            );
            AnsiConsole.WriteLine();

            List<string> actions = BuildQuestActions(
                selectedQuest
            );

            string action = inputReader.ReadSelection(
                "Choose an action:",
                actions,
                choice => choice
            );

            switch (action)
            {
                case "Edit":
                    EditQuest(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Change Project":
                    ChangeQuestProject(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Change Milestone":
                    ChangeQuestMilestone(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Score Positive":
                    CompleteQuest(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Archive":
                    ArchiveQuest(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Delete":
                    opened = !DeleteQuest(selectedQuest);
                    if (opened)
                    {
                        inputReader.WaitForContinue();
                    }
                    break;

                case "Back":
                    opened = false;
                    break;
            }
        }
    }

    private List<string> BuildQuestActions(Quest quest)
    {
        List<string> actions = [];

        if (quest.Status is QuestStatus.Created or QuestStatus.Active)
        {
            actions.Add("Edit");
            actions.Add("Change Project");
            actions.Add("Change Milestone");
        }

        if (quest.Status == QuestStatus.Active)
        {
            actions.Add("Score Positive");
        }

        if (quest.Status != QuestStatus.Archived)
        {
            actions.Add("Archive");
        }

        actions.Add("Delete");
        actions.Add("Back");
        return actions;
    }

    private void ListQuests()
    {
        ConsoleHelper.ShowHeader("Task Board");

        IReadOnlyList<QuestModel> quests =
            questService.GetAllQuests();

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No tasks have been created."
            );
            return;
        }

        QuestTable questTable = new(
            quests,
            GetProjectName
        );

        AnsiConsole.Write(questTable.Build());
    }

    private void EditQuest(Quest quest)
    {
        AnsiConsole.MarkupLine(
            $"[grey]Current title:[/] " +
            $"{Markup.Escape(quest.Title)}"
        );

        string title = inputReader.ReadRequiredString(
            "New title:"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Current description:[/] " +
            $"{Markup.Escape(quest.Description)}"
        );

        string description = inputReader.ReadRequiredString(
            "New description:"
        );

        if (!inputReader.ReadConfirmation(
            $"Save changes to '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Edit cancelled.");
            return;
        }

        questService.UpdateQuest(
            quest,
            title,
            description
        );
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Task updated successfully."
        );
    }

    private void ChangeQuestProject(Quest quest)
    {
        string currentProject = selectionFlow.GetProjectName(
            quest.ProjectId
        );

        AnsiConsole.MarkupLine(
            $"[grey]Current project:[/] " +
            $"{Markup.Escape(currentProject)}"
        );

        string option = inputReader.ReadSelection(
            "Choose an action:",
            new[]
            {
                "Assign to a Project",
                "Tornar independente",
                "Cancel"
            },
            choice => choice
        );

        bool changed = option switch
        {
            "Assign to a Project" =>
                AssignQuestToProject(quest),
            "Tornar independente" =>
                RemoveQuestFromProject(quest),
            _ => false
        };

        if (changed)
        {
            gameStateService.Save();
        }
        else if (option == "Cancel")
        {
            ConsoleHelper.ShowInformation(
                "Change cancelled."
            );
        }
    }

    private bool AssignQuestToProject(Quest quest)
    {
        List<Project> availableProjects = projectService
            .GetAllProjects()
            .Where(project =>
                project.Status is ProjectStatus.Created or
                    ProjectStatus.Active
            )
            .ToList();

        if (availableProjects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No projects are available."
            );
            return false;
        }

        Project selectedProject = inputReader.ReadSelection(
            "Select a project:",
            availableProjects,
            project => $"{project.Name} — {DisplayText.For(project.Status)}"
        );

        if (quest.MilestoneId is not null)
        {
            questService.RemoveQuestFromMilestone(quest);
        }

        questService.AssignQuestToProject(
            quest,
            selectedProject
        );
        ConsoleHelper.ShowSuccess(
            "Task assigned to the project successfully."
        );
        return true;
    }

    private bool RemoveQuestFromProject(Quest quest)
    {
        if (quest.ProjectId is null)
        {
            ConsoleHelper.ShowInformation(
                "The task is already independent."
            );
            return false;
        }

        if (quest.MilestoneId is not null)
        {
            questService.RemoveQuestFromMilestone(quest);
        }

        questService.RemoveQuestFromProject(quest);
        ConsoleHelper.ShowSuccess(
            "The project association was removed."
        );
        return true;
    }

    private void ChangeQuestMilestone(Quest quest)
    {
        if (quest.ProjectId is null)
        {
            ConsoleHelper.ShowInformation(
                "Associate the task with a project before selecting a milestone."
            );
            return;
        }

        Project? project = projectService.GetProjectById(quest.ProjectId.Value);
        if (project is null)
        {
            return;
        }

        string option = inputReader.ReadSelection(
            "Choose an action:",
            new[] { "Associate with a Milestone", "Remove Milestone", "Cancel" },
            choice => choice
        );

        if (option == "Associate with a Milestone")
        {
            Milestone? milestone = selectionFlow.SelectOptionalMilestone(project, requireConfirmation: false);
            if (milestone is not null)
            {
                if (quest.MilestoneId is not null)
                {
                    questService.RemoveQuestFromMilestone(quest);
                }

                questService.AssignQuestToMilestone(quest, milestone);
                gameStateService.Save();
                ConsoleHelper.ShowSuccess("Milestone association updated.");
            }
        }
        else if (option == "Remove Milestone" && quest.MilestoneId is not null)
        {
            questService.RemoveQuestFromMilestone(quest);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Milestone association removed.");
        }
    }

    private void CompleteQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Complete the task '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Task completion cancelled."
            );
            return;
        }

        QuestCompletionResult result =
            questWorkflowService.CompleteQuest(quest.Id);

        ConsoleHelper.ShowSuccess(
            "Task completed successfully."
        );

        ShowProjectProgress(result.Quest.ProjectId);

        if (result.MilestoneCompleted)
        {
            ConsoleHelper.ShowSuccess("The milestone was completed automatically.");
        }

        if (result.UnlockedBoss is not null)
        {
            ConsoleHelper.ShowSuccess(
                $"Boss unlocked: {result.UnlockedBoss.Name}."
            );
        }

        if (result.ActivatedMilestone is not null)
        {
            ConsoleHelper.ShowSuccess(
                $"Next milestone activated: {result.ActivatedMilestone.Title}."
            );
        }

        if (result.ActivatedQuest is not null)
        {
            ConsoleHelper.ShowSuccess(
                $"Next task activated: {result.ActivatedQuest.Title}."
            );
        }

        if (result.ProjectCompleted)
        {
            ConsoleHelper.ShowSuccess(
                "All valid tasks and milestones were completed. " +
                "The project was completed automatically."
            );
        }
    }

    private void ArchiveQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Archive the task '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Archiving canceled."
            );
            return;
        }

        questService.ArchiveQuest(quest);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Task archived successfully."
        );
    }

    private bool DeleteQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Permanently delete '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Deletion cancelled."
            );
            return false;
        }

        bool deleted = questService.DeleteQuest(quest.Id);

        if (!deleted)
        {
            ConsoleHelper.ShowError(
                "The task could not be deleted."
            );
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Task deleted successfully."
        );
        return true;
    }

    private QuestCard BuildQuestCard(Quest quest)
    {
        return new QuestCard(
            quest,
            selectionFlow.GetProjectName(
                quest.ProjectId
            )
        );
    }

    private string GetProjectName(int? projectId)
    {
        return selectionFlow.GetProjectName(
            projectId
        );
    }

    private void ShowProjectProgress(int? projectId)
    {
        if (projectId is null)
        {
            return;
        }

        Project? project =
            projectService.GetProjectById(
                projectId.Value
            );

        if (project is null)
        {
            return;
        }

        decimal progress =
            projectService.CalculateProgress(
                project,
                questService.GetAllQuests()
            );

        AnsiConsole.MarkupLine(
            $"[grey]Progress for " +
            $"{Markup.Escape(project.Name)}:[/] " +
            $"[green]{progress:0.##}%[/]"
        );
    }


}
