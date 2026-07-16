using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Milestone;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class MilestoneScreen
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly MilestoneWorkflowService milestoneWorkflowService;
    private readonly GameStateService gameStateService;
    private readonly InputReader inputReader;

    public MilestoneScreen(
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        MilestoneWorkflowService milestoneWorkflowService,
        GameStateService gameStateService,
        InputReader inputReader
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.milestoneWorkflowService = milestoneWorkflowService;
        this.gameStateService = gameStateService;
        this.inputReader = inputReader;
    }

    public void Show()
    {
        var projects = projectService.GetAllProjects();
        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation("Cadastre um projeto antes de criar capítulos.");
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
                new[] { "Novo capítulo", "Abrir capítulo", "Listar capítulos", "Voltar" },
                choice => choice
            );
            switch (option)
            {
                case "Novo capítulo": Create(project); inputReader.WaitForContinue(); break;
                case "Abrir capítulo": Open(project); break;
                case "Listar capítulos": List(project); inputReader.WaitForContinue(); break;
                case "Voltar": running = false; break;
            }
        }
    }

    private void Create(Project project)
    {
        ConsoleHelper.ShowHeader("Novo capítulo");
        inputReader.ShowCancellationHint();
        try
        {
            var existing = milestoneService.GetByProjectId(project.Id);
            int suggestedOrder = existing.Count == 0 ? 1 : existing.Max(item => item.Order) + 1;
            string title = inputReader.ReadRequiredStringOrCancel("Título:");
            string description = inputReader.ReadRequiredStringOrCancel("Descrição:");
            PromptDecision decision = inputReader.ReadDecision(
                "Usar uma quantidade específica de missões concluídas?"
            );
            if (decision == PromptDecision.Cancel) throw new UserCancelledException();
            int required = decision == PromptDecision.Yes
                ? inputReader.ReadPositiveIntegerOrCancel("Quantidade necessária de missões concluídas:")
                : 0;
            Milestone milestone = milestoneService.CreateMilestone(
                project, title, description, suggestedOrder, required
            );
            if (project.Status == ProjectStatus.Active && existing.Count == 0)
            {
                milestoneService.Activate(milestone);
            }
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Capítulo criado com sucesso.");
            AnsiConsole.Write(BuildCard(milestone).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Criação do capítulo cancelada.");
        }
    }

    private void Open(Project project)
    {
        var milestones = milestoneService.GetByProjectId(project.Id);
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
            if (milestone.Status == MilestoneStatus.Created) actions.Add("Ativar");
            if (milestone.Status == MilestoneStatus.Active &&
                questService.GetQuestsByMilestoneId(milestone.Id).Count == 0)
                actions.Add("Concluir manualmente");
            if (milestone.Status is not (MilestoneStatus.Completed or MilestoneStatus.Archived))
                actions.Add("Editar");
            if (milestone.Status != MilestoneStatus.Archived) actions.Add("Arquivar");
            actions.Add("Excluir");
            actions.Add("Voltar");
            string action = inputReader.ReadSelection("Escolha uma ação:", actions, choice => choice);
            switch (action)
            {
                case "Ver missões": ShowQuests(milestone); inputReader.WaitForContinue(); break;
                case "Ativar": milestoneService.Activate(milestone); gameStateService.Save(); break;
                case "Concluir manualmente": milestoneWorkflowService.CompleteManualMilestone(milestone.Id); break;
                case "Editar":
                    milestoneService.Update(
                        milestone,
                        inputReader.ReadRequiredString("Novo título:"),
                        inputReader.ReadRequiredString("Nova descrição:")
                    );
                    gameStateService.Save();
                    break;
                case "Arquivar": milestoneService.Archive(milestone); gameStateService.Save(); break;
                case "Excluir": opened = !milestoneWorkflowService.DeleteMilestone(milestone.Id); break;
                case "Voltar": opened = false; break;
            }
        }
    }

    private void List(Project project)
    {
        var milestones = milestoneService.GetByProjectId(project.Id);
        if (milestones.Count == 0)
        {
            ConsoleHelper.ShowInformation("Este projeto não possui capítulos.");
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
            ConsoleHelper.ShowInformation("Nenhuma missão está vinculada a este capítulo.");
            return;
        }
        Table table = new Table();
        table.AddColumn("Missão");
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
