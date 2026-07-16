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
using LevelUp.UI.Infrastructure;

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
            ConsoleHelper.ShowInformation("Crie um projeto antes de adicionar capítulos.");
            inputReader.WaitForContinue();
            return;
        }

        Project project = inputReader.ReadSelection(
            "Selecione um projeto:",
            projects,
            item => $"{item.Name} — {DisplayText.For(item.Status)}"
        );

        ShowForProject(project);
    }

    public void ShowForProject(Project project)
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader($"Capítulos — {project.Name}");
            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Novo capítulo",
                    "Abrir capítulo",
                    "Listar capítulos",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Novo capítulo":
                    Create(project);
                    inputReader.WaitForContinue();
                    break;
                case "Abrir capítulo":
                    Open(project);
                    break;
                case "Listar capítulos":
                    List(project);
                    inputReader.WaitForContinue();
                    break;
                case "Voltar":
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
            "Usar uma quantidade específica de missões concluídas?"
        )
            ? inputReader.ReadPositiveInteger("Quantidade necessária de missões concluídas:")
            : 0;

        MilestoneReward reward = ReadOptionalReward();

        Milestone milestone = milestoneService.CreateMilestone(
            project,
            inputReader.ReadRequiredString("Título:"),
            inputReader.ReadRequiredString("Descrição:"),
            suggestedOrder,
            requiredCompletedQuests,
            reward
        );

        if (project.Status == ProjectStatus.Active && existing.Count == 0)
        {
            milestoneService.Activate(milestone);
        }

        if (inputReader.ReadConfirmation("Adicionar um chefe final ao projeto?"))
        {
            bossService.Create(
                project,
                milestone,
                inputReader.ReadRequiredString("Nome do chefe:"),
                inputReader.ReadRequiredString("Descrição do chefe:"),
                isFinalBoss: inputReader.ReadConfirmation("Este é o chefe final do projeto?")
            );
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Capítulo criado com sucesso.");
        AnsiConsole.Write(BuildCard(milestone).Build());
    }

    private MilestoneReward ReadOptionalReward()
    {
        if (!inputReader.ReadConfirmation("Configurar uma recompensa opcional?"))
        {
            return new MilestoneReward();
        }

        int experience = inputReader.ReadConfirmation("Adicionar recompensa de XP?")
            ? inputReader.ReadPositiveInteger("Quantidade de XP:")
            : 0;
        int gold = 0;
        string? title = inputReader.ReadConfirmation("Adicionar um título desbloqueável?")
            ? inputReader.ReadRequiredString("Título da recompensa:")
            : null;

        return new MilestoneReward(experience, gold, title);
    }

    private void Open(Project project)
    {
        IReadOnlyList<Milestone> milestones = milestoneService.GetByProjectId(project.Id);
        if (milestones.Count == 0)
        {
            ConsoleHelper.ShowInformation("Este projeto não possui capítulos.");
            inputReader.WaitForContinue();
            return;
        }

        Milestone milestone = inputReader.ReadSelection(
            "Selecione um capítulo:",
            milestones,
            item => item.IsLocked
                ? $"{item.Order}. Capítulo bloqueado — {DisplayText.For(item.Status)}"
                : $"{item.Order}. {item.Title} — {DisplayText.For(item.Status)}"
        );

        bool opened = true;
        while (opened)
        {
            ConsoleHelper.ShowHeader("Capítulo");
            AnsiConsole.Write(BuildCard(milestone).Build());
            AnsiConsole.WriteLine();

            List<string> actions = ["Ver missões"];
            if (milestone.Status == MilestoneStatus.Created)
            {
                actions.Add("Ativar");
            }
            if (milestone.Status == MilestoneStatus.Active &&
                questService.GetQuestsByMilestoneId(milestone.Id).Count == 0)
            {
                actions.Add("Concluir manualmente");
            }
            BossEncounter? boss = bossService.GetByMilestoneId(milestone.Id);
            if (boss?.Status == BossStatus.Available)
            {
                actions.Add("Derrotar chefe");
            }
            if (milestone.Status is not (MilestoneStatus.Completed or MilestoneStatus.Archived))
            {
                actions.Add("Editar");
            }
            if (milestone.Status == MilestoneStatus.Completed &&
                milestone.Reward.HasReward &&
                milestone.RewardClaimedAt is null)
            {
                actions.Add("Resgatar recompensa");
            }
            if (milestone.Status != MilestoneStatus.Archived)
            {
                actions.Add("Arquivar");
            }
            actions.Add("Excluir");
            actions.Add("Voltar");

            string action = inputReader.ReadSelection("Escolha uma ação:", actions, choice => choice);
            switch (action)
            {
                case "Ver missões":
                    ShowQuests(milestone);
                    inputReader.WaitForContinue();
                    break;
                case "Ativar":
                    milestoneService.Activate(milestone);
                    gameStateService.Save();
                    break;
                case "Concluir manualmente":
                    milestoneWorkflowService.CompleteManualMilestone(milestone.Id);
                    break;
                case "Derrotar chefe":
                    bossWorkflowService.Defeat(milestone.Id);
                    break;
                case "Editar":
                    milestoneService.Update(
                        milestone,
                        inputReader.ReadRequiredString("Novo título:"),
                        inputReader.ReadRequiredString("Nova descrição:")
                    );
                    gameStateService.Save();
                    break;
                case "Resgatar recompensa":
                    milestone.ClaimReward();
                    gameStateService.Save();
                    ConsoleHelper.ShowSuccess("Recompensa resgatada. A entrega será integrada aos módulos de progressão e finanças.");
                    break;
                case "Arquivar":
                    milestoneService.Archive(milestone);
                    gameStateService.Save();
                    break;
                case "Excluir":
                    opened = !milestoneWorkflowService.DeleteMilestone(milestone.Id);
                    break;
                case "Voltar":
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
            ConsoleHelper.ShowInformation("Este projeto não possui capítulos.");
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
            ConsoleHelper.ShowInformation("Nenhuma missão está vinculada a este capítulo.");
            return;
        }

        Table table = new();
        table.AddColumn("Missão");
        table.AddColumn("Status");
        foreach (var quest in quests)
        {
            table.AddRow(Markup.Escape(quest.Title), DisplayText.For(quest.Status));
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
