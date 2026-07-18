using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Milestone;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class MilestoneScreen
{
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly MilestoneWorkflowService milestoneWorkflowService;
    private readonly GameStateService gameStateService;
    private readonly InputReader inputReader;

    public MilestoneScreen(
        QuestService questService,
        MilestoneService milestoneService,
        MilestoneWorkflowService milestoneWorkflowService,
        GameStateService gameStateService,
        InputReader inputReader
    )
    {
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.milestoneWorkflowService = milestoneWorkflowService;
        this.gameStateService = gameStateService;
        this.inputReader = inputReader;
    }

    public void ShowForProject(Project project)
    {
        bool running = true;
        while (running)
        {
            ConsoleHelper.ShowHeader($"Milestones — {project.Name}");
            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[] { "New Milestone", "Open Milestone", "List Milestones", "Back" },
                choice => choice
            );
            switch (option)
            {
                case "New Milestone": Create(project); inputReader.WaitForContinue(); break;
                case "Open Milestone": Open(project); break;
                case "List Milestones": List(project); inputReader.WaitForContinue(); break;
                case "Back": running = false; break;
            }
        }
    }

    private void Create(Project project)
    {
        ConsoleHelper.ShowHeader("New Milestone");
        inputReader.ShowCancellationHint();
        try
        {
            var existing = milestoneService.GetByProjectId(project.Id);
            int suggestedOrder = existing.Count == 0 ? 1 : existing.Max(item => item.Order) + 1;
            string title = inputReader.ReadRequiredStringOrCancel("Title:");
            string description = inputReader.ReadRequiredStringOrCancel("Description:");
            PromptDecision decision = inputReader.ReadDecision(
                "Require a specific number of completed tasks?"
            );
            if (decision == PromptDecision.Cancel) throw new UserCancelledException();
            int required = decision == PromptDecision.Yes
                ? inputReader.ReadPositiveIntegerOrCancel("Required completed tasks:")
                : 0;
            Milestone milestone = milestoneService.CreateMilestone(
                project, title, description, suggestedOrder, required
            );
            if (project.Status == ProjectStatus.Active && existing.Count == 0)
            {
                milestoneService.Activate(milestone);
            }
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Milestone created successfully.");
            AnsiConsole.Write(BuildCard(milestone).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Milestone creation cancelled.");
        }
    }

    private void Open(Project project)
    {
        var milestones = milestoneService.GetByProjectId(project.Id);
        if (milestones.Count == 0)
        {
            ConsoleHelper.ShowInformation("This project has no milestones.");
            inputReader.WaitForContinue();
            return;
        }
        Milestone milestone = inputReader.ReadSelection(
            "Select a milestone:",
            milestones,
            item => item.IsLocked
                ? $"{item.Order}. Locked milestone — {DisplayText.For(item.Status)}"
                : $"{item.Order}. {item.Title} — {DisplayText.For(item.Status)}"
        );
        bool opened = true;
        while (opened)
        {
            ConsoleHelper.ShowHeader("Milestone");
            AnsiConsole.Write(BuildCard(milestone).Build());
            AnsiConsole.WriteLine();
            List<string> actions = ["Ver tasks"];
            if (milestone.Status == MilestoneStatus.Created) actions.Add("Activer");
            if (milestone.Status == MilestoneStatus.Active &&
                questService.GetQuestsByMilestoneId(milestone.Id).Count == 0)
                actions.Add("Score Positive manualmente");
            if (milestone.Status is not (MilestoneStatus.Completed or MilestoneStatus.Archived))
                actions.Add("Edit");
            if (milestone.Status != MilestoneStatus.Archived) actions.Add("Archive");
            actions.Add("Delete");
            actions.Add("Back");
            string action = inputReader.ReadSelection("Choose an action:", actions, choice => choice);
            switch (action)
            {
                case "Ver tasks": ShowQuests(milestone); inputReader.WaitForContinue(); break;
                case "Activer": milestoneService.Activate(milestone); gameStateService.Save(); break;
                case "Score Positive manualmente": milestoneWorkflowService.CompleteManualMilestone(milestone.Id); break;
                case "Edit":
                    milestoneService.Update(
                        milestone,
                        inputReader.ReadRequiredString("New title:"),
                        inputReader.ReadRequiredString("New description:")
                    );
                    gameStateService.Save();
                    break;
                case "Archive": milestoneService.Archive(milestone); gameStateService.Save(); break;
                case "Delete": opened = !milestoneWorkflowService.DeleteMilestone(milestone.Id); break;
                case "Back": opened = false; break;
            }
        }
    }

    private void List(Project project)
    {
        var milestones = milestoneService.GetByProjectId(project.Id);
        if (milestones.Count == 0)
        {
            ConsoleHelper.ShowInformation("This project has no milestones.");
            return;
        }
        AnsiConsole.Write(new MilestoneTable(
            milestones,
            milestone => milestoneService.CalculateProgress(milestone, questService.GetAllQuests())
        ).Build());
    }

    private void ShowQuests(Milestone milestone)
    {
        var quests = questService.GetQuestsByMilestoneId(milestone.Id);
        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation("No tasks are linked to this milestone.");
            return;
        }
        Table table = new Table();
        table.AddColumn("Task");
        table.AddColumn("Status");
        foreach (var quest in quests)
            table.AddRow(Markup.Escape(quest.Title), DisplayText.For(quest.Status));
        AnsiConsole.Write(table);
    }

    private MilestoneCard BuildCard(Milestone milestone) => new(
        milestone,
        questService.GetQuestsByMilestoneId(milestone.Id),
        milestoneService.CalculateProgress(milestone, questService.GetAllQuests())
    );
}
