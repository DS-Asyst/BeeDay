using LevelUp.Domain.Bosses;
using LevelUp.Domain.Projects;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Project;
using LevelUp.UI.Components.Quest;
using LevelUp.UI.Infrastructure;
using Spectre.Console;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI;

public sealed class ProjectScreen
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly InputReader inputReader;
    private readonly GameStateService gameStateService;
    private readonly ProjectWorkflowService projectWorkflowService;
    private readonly MilestoneService milestoneService;
    private readonly MilestoneScreen milestoneScreen;
    private readonly BossService bossService;
    private readonly BossWorkflowService bossWorkflowService;

    public ProjectScreen(
        ProjectService projectService,
        QuestService questService,
        InputReader inputReader,
        GameStateService gameStateService,
        ProjectWorkflowService projectWorkflowService,
        MilestoneService milestoneService,
        MilestoneScreen milestoneScreen,
        BossService bossService,
        BossWorkflowService bossWorkflowService
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.inputReader = inputReader;
        this.gameStateService = gameStateService;
        this.projectWorkflowService = projectWorkflowService;
        this.milestoneService = milestoneService;
        this.milestoneScreen = milestoneScreen;
        this.bossService = bossService;
        this.bossWorkflowService = bossWorkflowService;
    }

    public void Show()
    {
        bool running = true;
        while (running)
        {
            ConsoleHelper.ShowHeader("Painel de Projetos");
            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[] { "Novo projeto", "Abrir projeto", "Listar projetos", "Voltar" },
                choice => choice
            );
            switch (option)
            {
                case "Novo projeto": CreateProject(); inputReader.WaitForContinue(); break;
                case "Abrir projeto": OpenProject(); break;
                case "Listar projetos": ListProjects(); inputReader.WaitForContinue(); break;
                case "Voltar": running = false; break;
            }
        }
    }

    private void CreateProject()
    {
        ConsoleHelper.ShowHeader("Novo projeto");
        inputReader.ShowCancellationHint();
        try
        {
            string name = inputReader.ReadRequiredStringOrCancel("Nome:");
            string description = inputReader.ReadRequiredStringOrCancel("Descrição:");
            string bossName = inputReader.ReadRequiredStringOrCancel("Nome do chefe final:");
            string bossDescription = inputReader.ReadRequiredStringOrCancel("Descrição do chefe final:");
            string achievementPrefix = inputReader.ReadRequiredStringOrCancel(
                "Prefixo da conquista (ex.: Desenvolvedor):"
            );
            Project project = projectWorkflowService.CreateProject(
                name, description, bossName, bossDescription, achievementPrefix
            );
            ConsoleHelper.ShowSuccess("Projeto e chefe final criados com sucesso.");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(BuildProjectCard(project).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Criação do projeto cancelada.");
        }
    }

    private void OpenProject()
    {
        var projects = projectService.GetAllProjects();
        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhum projeto foi cadastrado.");
            inputReader.WaitForContinue();
            return;
        }
        Project project = SelectProject("Selecione um projeto:", projects);
        bool opened = true;
        while (opened)
        {
            ConsoleHelper.ShowHeader("Projeto");
            AnsiConsole.Write(BuildProjectCard(project).Build());
            AnsiConsole.WriteLine();
            string action = inputReader.ReadSelection(
                "Escolha uma ação:", BuildProjectActions(project), choice => choice
            );
            switch (action)
            {
                case "Ativar": ActivateProject(project); inputReader.WaitForContinue(); break;
                case "Editar": EditProject(project); inputReader.WaitForContinue(); break;
                case "Ver missões": ShowProjectQuests(project); inputReader.WaitForContinue(); break;
                case "Ver capítulos": milestoneScreen.ShowForProject(project); break;
                case "Enfrentar chefe final": DefeatBoss(project); inputReader.WaitForContinue(); break;
                case "Arquivar": ArchiveProject(project); inputReader.WaitForContinue(); break;
                case "Excluir": opened = !DeleteProject(project); if (opened) inputReader.WaitForContinue(); break;
                case "Voltar": opened = false; break;
            }
        }
    }

    private List<string> BuildProjectActions(Project project)
    {
        List<string> actions = [];
        if (project.Status == ProjectStatus.Created) actions.Add("Ativar");
        if (project.Status != ProjectStatus.Archived) actions.Add("Editar");
        actions.Add("Ver missões");
        actions.Add("Ver capítulos");
        if (bossService.GetByProjectId(project.Id)?.Status == BossStatus.Available)
            actions.Add("Enfrentar chefe final");
        if (project.Status != ProjectStatus.Archived) actions.Add("Arquivar");
        actions.Add("Excluir");
        actions.Add("Voltar");
        return actions;
    }

    private void ListProjects()
    {
        var projects = projectService.GetAllProjects();
        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhum projeto foi cadastrado.");
            return;
        }
        AnsiConsole.Write(new ProjectTable(
            projects,
            questService.GetQuestsByProjectId,
            CalculateProgress,
            milestoneService.GetByProjectId
        ).Build());
    }

    private void ActivateProject(Project project)
    {
        if (!inputReader.ReadConfirmation($"Ativar '{project.Name}'?"))
        {
            ConsoleHelper.ShowInformation("Ativação cancelada.");
            return;
        }
        projectService.ActivateProject(project);
        milestoneService.TryActivateFirst(project);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Projeto ativado com sucesso.");
    }

    private void EditProject(Project project)
    {
        BossEncounter boss = bossService.GetByProjectId(project.Id)
            ?? throw new InvalidOperationException("O projeto não possui chefe final.");
        inputReader.ShowCancellationHint();
        try
        {
            string name = inputReader.ReadRequiredStringOrCancel("Novo nome:");
            string description = inputReader.ReadRequiredStringOrCancel("Nova descrição:");
            string bossName = inputReader.ReadRequiredStringOrCancel("Novo nome do chefe final:");
            string bossDescription = inputReader.ReadRequiredStringOrCancel("Nova descrição do chefe final:");
            string prefix = inputReader.ReadRequiredStringOrCancel("Novo prefixo da conquista:");
            if (!inputReader.ReadConfirmation($"Salvar alterações em '{project.Name}'?"))
            {
                ConsoleHelper.ShowInformation("Edição cancelada.");
                return;
            }
            projectService.UpdateProject(project, name, description);
            bossService.Update(boss, bossName, bossDescription, prefix);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Projeto atualizado com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Edição cancelada.");
        }
    }

    private void DefeatBoss(Project project)
    {
        var result = bossWorkflowService.Defeat(project.Id);
        ConsoleHelper.ShowSuccess(
            $"Chefe derrotado. Conquista desbloqueada: {result.Achievement.Name}."
        );
    }

    private void ShowProjectQuests(Project project)
    {
        IReadOnlyList<QuestModel> quests = questService.GetQuestsByProjectId(project.Id);
        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation("Este projeto ainda não possui missões.");
            return;
        }
        AnsiConsole.Write(new QuestTable(quests, _ => project.Name).Build());
    }

    private void ArchiveProject(Project project)
    {
        if (!inputReader.ReadConfirmation($"Arquivar '{project.Name}'?"))
        {
            ConsoleHelper.ShowInformation("Arquivamento cancelado.");
            return;
        }
        projectService.ArchiveProject(project);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Projeto arquivado com sucesso.");
    }

    private bool DeleteProject(Project project)
    {
        var linkedQuests = questService.GetQuestsByProjectId(project.Id);
        if (linkedQuests.Count > 0 && !inputReader.ReadConfirmation(
            $"O projeto possui {linkedQuests.Count} missão(ões). Torná-las independentes?"
        )) return false;
        if (!inputReader.ReadConfirmation($"Excluir permanentemente '{project.Name}'?")) return false;
        bool deleted = projectWorkflowService.DeleteProject(project.Id);
        if (deleted) ConsoleHelper.ShowSuccess("Projeto excluído com sucesso.");
        return deleted;
    }

    private Project SelectProject(string prompt, IEnumerable<Project> projects)
    {
        return inputReader.ReadSelection(
            prompt, projects, project => $"{project.Name} — {DisplayText.For(project.Status)}"
        );
    }

    private decimal CalculateProgress(Project project) =>
        projectService.CalculateProgress(project, questService.GetAllQuests());

    private ProjectCard BuildProjectCard(Project project) => new(
        project,
        questService.GetQuestsByProjectId(project.Id),
        CalculateProgress(project),
        milestoneService.GetByProjectId(project.Id),
        bossService.GetByProjectId(project.Id)
    );
}
