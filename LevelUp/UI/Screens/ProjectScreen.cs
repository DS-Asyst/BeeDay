using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Project;
using LevelUp.UI.Components.Quest;
using Spectre.Console;
using QuestModel = LevelUp.Domain.Quests.Quest;
using LevelUp.UI.Infrastructure;

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

    public ProjectScreen(
        ProjectService projectService,
        QuestService questService,
        InputReader inputReader,
        GameStateService gameStateService,
        ProjectWorkflowService projectWorkflowService,
        MilestoneService milestoneService,
        MilestoneScreen milestoneScreen
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.inputReader = inputReader;
        this.gameStateService = gameStateService;
        this.projectWorkflowService = projectWorkflowService;
        this.milestoneService = milestoneService;
        this.milestoneScreen = milestoneScreen;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Painel de Projetos");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Novo projeto",
                    "Abrir projeto",
                    "Listar projetos",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Novo projeto":
                    CreateProject();
                    inputReader.WaitForContinue();
                    break;

                case "Abrir projeto":
                    OpenProject();
                    break;

                case "Listar projetos":
                    ListProjects();
                    inputReader.WaitForContinue();
                    break;

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private void CreateProject()
    {
        ConsoleHelper.ShowHeader("Novo projeto");

        Project project = projectService.CreateProject(
            inputReader.ReadRequiredString("Nome:"),
            inputReader.ReadRequiredString("Descrição:"),
            inputReader.ReadRequiredString(
                "Título desbloqueado:"
            )
        );

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Projeto criado com sucesso."
        );
        AnsiConsole.WriteLine();
        AnsiConsole.Write(BuildProjectCard(project).Build());
    }

    private void OpenProject()
    {
        IReadOnlyList<Project> projects =
            projectService.GetAllProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum projeto foi cadastrado."
            );
            inputReader.WaitForContinue();
            return;
        }

        Project project = SelectProject(
            "Selecione um projeto:",
            projects
        );
        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Projeto");
            AnsiConsole.Write(BuildProjectCard(project).Build());
            AnsiConsole.WriteLine();

            string action = inputReader.ReadSelection(
                "Escolha uma ação:",
                BuildProjectActions(project),
                choice => choice
            );

            switch (action)
            {
                case "Ativar":
                    ActivateProject(project);
                    inputReader.WaitForContinue();
                    break;

                case "Editar":
                    EditProject(project);
                    inputReader.WaitForContinue();
                    break;

                case "Ver missões":
                    ShowProjectQuests(project);
                    inputReader.WaitForContinue();
                    break;

                case "Ver capítulos":
                    milestoneScreen.ShowForProject(project);
                    break;

                case "Arquivar":
                    ArchiveProject(project);
                    inputReader.WaitForContinue();
                    break;

                case "Excluir":
                    opened = !DeleteProject(project);
                    if (opened)
                    {
                        inputReader.WaitForContinue();
                    }
                    break;

                case "Voltar":
                    opened = false;
                    break;
            }
        }
    }

    private List<string> BuildProjectActions(Project project)
    {
        List<string> actions = [];

        if (project.Status == ProjectStatus.Created)
        {
            actions.Add("Ativar");
        }

        if (project.Status != ProjectStatus.Archived)
        {
            actions.Add("Editar");
        }

        actions.Add("Ver missões");
        actions.Add("Ver capítulos");

        if (project.Status != ProjectStatus.Archived)
        {
            actions.Add("Arquivar");
        }

        actions.Add("Excluir");
        actions.Add("Voltar");
        return actions;
    }

    private void ListProjects()
    {
        IReadOnlyList<Project> projects =
            projectService.GetAllProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum projeto foi cadastrado."
            );
            return;
        }

        ProjectTable table = new(
            projects,
            questService.GetQuestsByProjectId,
            CalculateProgress,
            milestoneService.GetByProjectId
        );

        AnsiConsole.Write(table.Build());
    }

    private void ActivateProject(Project project)
    {
        if (!inputReader.ReadConfirmation(
            $"Ativar '{project.Name}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Ativação cancelada."
            );
            return;
        }

        projectService.ActivateProject(project);
        milestoneService.TryActivateFirst(project);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Projeto ativado com sucesso."
        );
    }

    private void EditProject(Project project)
    {
        AnsiConsole.MarkupLine(
            $"[grey]Nome atual:[/] " +
            $"{Markup.Escape(project.Name)}"
        );
        string name = inputReader.ReadRequiredString(
            "Novo nome:"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Descrição atual:[/] " +
            $"{Markup.Escape(project.Description)}"
        );
        string description = inputReader.ReadRequiredString(
            "Nova descrição:"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Título atual:[/] " +
            $"{Markup.Escape(project.UnlockedTitle)}"
        );
        string unlockedTitle = inputReader.ReadRequiredString(
            "Novo título desbloqueado:"
        );

        if (!inputReader.ReadConfirmation(
            $"Salvar alterações em '{project.Name}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Edição cancelada.");
            return;
        }

        projectService.UpdateProject(
            project,
            name,
            description,
            unlockedTitle
        );
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Projeto atualizado com sucesso."
        );
    }

    private void ShowProjectQuests(Project project)
    {
        IReadOnlyList<QuestModel> quests =
            questService.GetQuestsByProjectId(project.Id);

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Este projeto ainda não possui missões."
            );
            return;
        }

        QuestTable table = new(
            quests,
            _ => project.Name
        );
        AnsiConsole.Write(table.Build());
    }

    private void ArchiveProject(Project project)
    {
        if (!inputReader.ReadConfirmation(
            $"Arquivar '{project.Name}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Arquivamento cancelado."
            );
            return;
        }

        projectService.ArchiveProject(project);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Projeto arquivado com sucesso."
        );
    }

    private bool DeleteProject(Project project)
    {
        IReadOnlyList<QuestModel> linkedQuests =
            questService.GetQuestsByProjectId(project.Id);

        if (linkedQuests.Count > 0 &&
            !inputReader.ReadConfirmation(
                $"O projeto possui {linkedQuests.Count} missão(ões). " +
                "Torná-las independentes?"
            ))
        {
            ConsoleHelper.ShowInformation(
                "Exclusão cancelada."
            );
            return false;
        }

        if (!inputReader.ReadConfirmation(
            $"Excluir permanentemente '{project.Name}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Exclusão cancelada."
            );
            return false;
        }

        if (!projectWorkflowService.DeleteProject(project.Id))
        {
            ConsoleHelper.ShowError(
                "Não foi possível excluir o projeto."
            );
            return false;
        }

        ConsoleHelper.ShowSuccess(
            "Projeto excluído com sucesso."
        );
        return true;
    }

    private Project SelectProject(
        string prompt,
        IEnumerable<Project> projects
    )
    {
        return inputReader.ReadSelection(
            prompt,
            projects,
            project => $"{project.Name} — {DisplayText.For(project.Status)}"
        );
    }

    private decimal CalculateProgress(Project project)
    {
        return projectService.CalculateProgress(
            project,
            questService.GetAllQuests()
        );
    }

    private ProjectCard BuildProjectCard(Project project)
    {
        return new ProjectCard(
            project,
            questService.GetQuestsByProjectId(project.Id),
            CalculateProgress(project),
            milestoneService.GetByProjectId(project.Id)
        );
    }
}
