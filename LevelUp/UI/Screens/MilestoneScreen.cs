using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Milestone;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class MilestoneScreen
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly BossService bossService;
    private readonly MilestoneWorkflowService milestoneWorkflowService;
    private readonly BossWorkflowService bossWorkflowService;
    private readonly GameStateService gameStateService;
    private readonly InputReader inputReader;

    public MilestoneScreen(
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        BossService bossService,
        MilestoneWorkflowService milestoneWorkflowService,
        BossWorkflowService bossWorkflowService,
        GameStateService gameStateService,
        InputReader inputReader
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.bossService = bossService;
        this.milestoneWorkflowService = milestoneWorkflowService;
        this.bossWorkflowService = bossWorkflowService;
        this.gameStateService = gameStateService;
        this.inputReader = inputReader;
    }

    public void Show()
    {
        List<Project> projects = projectService.GetAllProjects()
            .Where(project => project.Status != ProjectStatus.Archived)
            .ToList();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation("Create a project before adding milestones.");
            inputReader.WaitForContinue();
            return;
        }

        Project project = inputReader.ReadSelection(
            "Select a project:",
            projects,
            item => $"{item.Name} — {item.Status}"
        );

        ShowForProject(project);
    }

    public void ShowForProject(Project project)
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader($"Milestones — {project.Name}");
            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "New milestone",
                    "Open milestone",
                    "List milestones",
                    "Back"
                },
                choice => choice
            );

            switch (option)
            {
                case "New milestone":
                    Create(project);
                    inputReader.WaitForContinue();
                    break;
                case "Open milestone":
                    Open(project);
                    break;
                case "List milestones":
                    List(project);
                    inputReader.WaitForContinue();
                    break;
                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private void Create(Project project)
    {
        IReadOnlyList<Milestone> existing = milestoneService.GetByProjectId(project.Id);
        int suggestedOrder = existing.Count == 0 ? 1 : existing.Max(item => item.Order) + 1;

        int requiredCompletedQuests = inputReader.ReadConfirmation(
            "Use a specific completed-quest target?"
        )
            ? inputReader.ReadPositiveInteger("Required completed quests:")
            : 0;

        MilestoneReward reward = ReadOptionalReward();

        Milestone milestone = milestoneService.CreateMilestone(
            project,
            inputReader.ReadRequiredString("Title:"),
            inputReader.ReadRequiredString("Description:"),
            suggestedOrder,
            requiredCompletedQuests,
            reward
        );

        if (project.Status == ProjectStatus.Active && existing.Count == 0)
        {
            milestoneService.Activate(milestone);
        }

        if (inputReader.ReadConfirmation("Add an optional boss encounter?"))
        {
            bossService.Create(
                project,
                milestone,
                inputReader.ReadRequiredString("Boss name:"),
                inputReader.ReadRequiredString("Boss description:"),
                isFinalBoss: inputReader.ReadConfirmation("Is this the final project boss?")
            );
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Milestone created successfully.");
        AnsiConsole.Write(BuildCard(milestone).Build());
    }

    private MilestoneReward ReadOptionalReward()
    {
        if (!inputReader.ReadConfirmation("Configure an optional reward?"))
        {
            return new MilestoneReward();
        }

        int experience = inputReader.ReadConfirmation("Add an XP reward?")
            ? inputReader.ReadPositiveInteger("XP amount:")
            : 0;
        int gold = inputReader.ReadConfirmation("Add a Gold reward?")
            ? inputReader.ReadPositiveInteger("Gold amount:")
            : 0;
        string? title = inputReader.ReadConfirmation("Add an unlockable title?")
            ? inputReader.ReadRequiredString("Title reward:")
            : null;

        return new MilestoneReward(experience, gold, title);
    }

    private void Open(Project project)
    {
        IReadOnlyList<Milestone> milestones = milestoneService.GetByProjectId(project.Id);
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
                ? $"{item.Order}. Locked milestone — {item.Status}"
                : $"{item.Order}. {item.Title} — {item.Status}"
        );

        bool opened = true;
        while (opened)
        {
            ConsoleHelper.ShowHeader("Milestone");
            AnsiConsole.Write(BuildCard(milestone).Build());
            AnsiConsole.WriteLine();

            List<string> actions = ["View quests"];
            if (milestone.Status == MilestoneStatus.Created)
            {
                actions.Add("Activate");
            }
            if (milestone.Status == MilestoneStatus.Active &&
                questService.GetQuestsByMilestoneId(milestone.Id).Count == 0)
            {
                actions.Add("Complete manually");
            }
            BossEncounter? boss = bossService.GetByMilestoneId(milestone.Id);
            if (boss?.Status == BossStatus.Available)
            {
                actions.Add("Defeat boss");
            }
            if (milestone.Status is not (MilestoneStatus.Completed or MilestoneStatus.Archived))
            {
                actions.Add("Edit");
            }
            if (milestone.Status == MilestoneStatus.Completed &&
                milestone.Reward.HasReward &&
                milestone.RewardClaimedAt is null)
            {
                actions.Add("Claim reward");
            }
            if (milestone.Status != MilestoneStatus.Archived)
            {
                actions.Add("Archive");
            }
            actions.Add("Delete");
            actions.Add("Back");

            string action = inputReader.ReadSelection("Choose an action:", actions, choice => choice);
            switch (action)
            {
                case "View quests":
                    ShowQuests(milestone);
                    inputReader.WaitForContinue();
                    break;
                case "Activate":
                    milestoneService.Activate(milestone);
                    gameStateService.Save();
                    break;
                case "Complete manually":
                    milestoneWorkflowService.CompleteManualMilestone(milestone.Id);
                    break;
                case "Defeat boss":
                    bossWorkflowService.Defeat(milestone.Id);
                    break;
                case "Edit":
                    milestoneService.Update(
                        milestone,
                        inputReader.ReadRequiredString("New title:"),
                        inputReader.ReadRequiredString("New description:")
                    );
                    gameStateService.Save();
                    break;
                case "Claim reward":
                    milestone.ClaimReward();
                    gameStateService.Save();
                    ConsoleHelper.ShowSuccess("Reward claimed. Delivery will be integrated with Gold and progression modules.");
                    break;
                case "Archive":
                    milestoneService.Archive(milestone);
                    gameStateService.Save();
                    break;
                case "Delete":
                    opened = !milestoneWorkflowService.DeleteMilestone(milestone.Id);
                    break;
                case "Back":
                    opened = false;
                    break;
            }
        }
    }

    private void List(Project project)
    {
        IReadOnlyList<Milestone> milestones = milestoneService.GetByProjectId(project.Id);
        if (milestones.Count == 0)
        {
            ConsoleHelper.ShowInformation("This project has no milestones.");
            return;
        }

        MilestoneTable table = new(
            milestones,
            milestone => milestoneService.CalculateProgress(
                milestone,
                questService.GetAllQuests()
            )
        );
        AnsiConsole.Write(table.Build());
    }

    private void ShowQuests(Milestone milestone)
    {
        var quests = questService.GetQuestsByMilestoneId(milestone.Id);
        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation("No quests are linked to this milestone.");
            return;
        }

        Table table = new();
        table.AddColumn("Quest");
        table.AddColumn("Status");
        foreach (var quest in quests)
        {
            table.AddRow(Markup.Escape(quest.Title), quest.Status.ToString());
        }
        AnsiConsole.Write(table);
    }

    private MilestoneCard BuildCard(Milestone milestone)
    {
        BossEncounter? boss = bossService.GetByMilestoneId(milestone.Id);
        return new MilestoneCard(
            milestone,
            questService.GetQuestsByMilestoneId(milestone.Id),
            boss,
            milestoneService.CalculateProgress(milestone, questService.GetAllQuests())
        );
    }
}
