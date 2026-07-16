using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Quest;
using Spectre.Console;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.UI;

public sealed class QuestScreen
{
    private readonly QuestService questService;
    private readonly ProjectService projectService;
    private readonly InputReader inputReader;
    private readonly GameStateService gameStateService;
    private readonly QuestWorkflowService questWorkflowService;
    private readonly MilestoneService milestoneService;

    public QuestScreen(
        QuestService questService,
        ProjectService projectService,
        InputReader inputReader,
        GameStateService gameStateService,
        QuestWorkflowService questWorkflowService,
        MilestoneService milestoneService
    )
    {
        this.questService = questService;
        this.projectService = projectService;
        this.inputReader = inputReader;
        this.gameStateService = gameStateService;
        this.questWorkflowService = questWorkflowService;
        this.milestoneService = milestoneService;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Quest Board");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Nova quest",
                    "Abrir quest",
                    "Listar quests",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Nova quest":
                    CreateQuest();
                    inputReader.WaitForContinue();
                    break;

                case "Abrir quest":
                    OpenQuest();
                    break;

                case "Listar quests":
                    ListQuests();
                    inputReader.WaitForContinue();
                    break;

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private void CreateQuest()
    {
        ConsoleHelper.ShowHeader("Nova quest");

        string title = inputReader.ReadRequiredString("Título:");
        string description = inputReader.ReadRequiredString(
            "Descrição:"
        );
        Project? project = SelectOptionalProject();

        Quest quest = questService.CreateQuest(
            title,
            description,
            project
        );

        if (project is not null)
        {
            Milestone? milestone = SelectOptionalMilestone(project);
            if (milestone is not null)
            {
                questService.AssignQuestToMilestone(quest, milestone);
            }
        }

        questService.ActivateQuest(quest);
        gameStateService.Save();

        ConsoleHelper.ShowSuccess(
            "Quest cadastrada com sucesso."
        );

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            BuildQuestCard(quest).Build()
        );
    }

    private void OpenQuest()
    {
        IReadOnlyList<QuestModel> quests =
            questService.GetAllQuests();

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhuma quest foi cadastrada."
            );
            inputReader.WaitForContinue();
            return;
        }

        Quest selectedQuest = SelectQuest(
            "Selecione uma quest:",
            quests
        );

        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Quest");
            AnsiConsole.Write(
                BuildQuestCard(selectedQuest).Build()
            );
            AnsiConsole.WriteLine();

            List<string> actions = BuildQuestActions(
                selectedQuest
            );

            string action = inputReader.ReadSelection(
                "Escolha uma ação:",
                actions,
                choice => choice
            );

            switch (action)
            {
                case "Editar":
                    EditQuest(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Alterar projeto":
                    ChangeQuestProject(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Alterar milestone":
                    ChangeQuestMilestone(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Concluir":
                    CompleteQuest(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Arquivar":
                    ArchiveQuest(selectedQuest);
                    inputReader.WaitForContinue();
                    break;

                case "Excluir":
                    opened = !DeleteQuest(selectedQuest);
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

    private List<string> BuildQuestActions(Quest quest)
    {
        List<string> actions = [];

        if (quest.Status is QuestStatus.Created or QuestStatus.Active)
        {
            actions.Add("Editar");
            actions.Add("Alterar projeto");
            actions.Add("Alterar milestone");
        }

        if (quest.Status == QuestStatus.Active)
        {
            actions.Add("Concluir");
        }

        if (quest.Status != QuestStatus.Archived)
        {
            actions.Add("Arquivar");
        }

        actions.Add("Excluir");
        actions.Add("Voltar");
        return actions;
    }

    private void ListQuests()
    {
        ConsoleHelper.ShowHeader("Quest Board");

        IReadOnlyList<QuestModel> quests =
            questService.GetAllQuests();

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhuma quest foi cadastrada."
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
            $"[grey]Título atual:[/] " +
            $"{Markup.Escape(quest.Title)}"
        );

        string title = inputReader.ReadRequiredString(
            "Novo título:"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Descrição atual:[/] " +
            $"{Markup.Escape(quest.Description)}"
        );

        string description = inputReader.ReadRequiredString(
            "Nova descrição:"
        );

        if (!inputReader.ReadConfirmation(
            $"Salvar alterações em '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Edição cancelada.");
            return;
        }

        questService.UpdateQuest(
            quest,
            title,
            description
        );
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Quest atualizada com sucesso."
        );
    }

    private void ChangeQuestProject(Quest quest)
    {
        string currentProject = GetProjectName(
            quest.ProjectId
        );

        AnsiConsole.MarkupLine(
            $"[grey]Projeto atual:[/] " +
            $"{Markup.Escape(currentProject)}"
        );

        string option = inputReader.ReadSelection(
            "Escolha uma ação:",
            new[]
            {
                "Associar a um projeto",
                "Tornar independente",
                "Cancelar"
            },
            choice => choice
        );

        bool changed = option switch
        {
            "Associar a um projeto" =>
                AssignQuestToProject(quest),
            "Tornar independente" =>
                RemoveQuestFromProject(quest),
            _ => false
        };

        if (changed)
        {
            gameStateService.Save();
        }
        else if (option == "Cancelar")
        {
            ConsoleHelper.ShowInformation(
                "Alteração cancelada."
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
                "Não existem projetos disponíveis."
            );
            return false;
        }

        Project selectedProject = inputReader.ReadSelection(
            "Selecione o projeto:",
            availableProjects,
            project => $"{project.Name} — {project.Status}"
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
            "Quest associada ao projeto com sucesso."
        );
        return true;
    }

    private bool RemoveQuestFromProject(Quest quest)
    {
        if (quest.ProjectId is null)
        {
            ConsoleHelper.ShowInformation(
                "A quest já é independente."
            );
            return false;
        }

        if (quest.MilestoneId is not null)
        {
            questService.RemoveQuestFromMilestone(quest);
        }

        questService.RemoveQuestFromProject(quest);
        ConsoleHelper.ShowSuccess(
            "A associação com o projeto foi removida."
        );
        return true;
    }

    private void ChangeQuestMilestone(Quest quest)
    {
        if (quest.ProjectId is null)
        {
            ConsoleHelper.ShowInformation(
                "Associate the quest with a project before selecting a milestone."
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
            new[] { "Associate with milestone", "Remove milestone", "Cancel" },
            choice => choice
        );

        if (option == "Associate with milestone")
        {
            Milestone? milestone = SelectOptionalMilestone(project, requireConfirmation: false);
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
        else if (option == "Remove milestone" && quest.MilestoneId is not null)
        {
            questService.RemoveQuestFromMilestone(quest);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Milestone association removed.");
        }
    }

    private Milestone? SelectOptionalMilestone(
        Project project,
        bool requireConfirmation = true
    )
    {
        List<Milestone> milestones = milestoneService
            .GetByProjectId(project.Id)
            .Where(milestone => milestone.CanAcceptQuests)
            .ToList();

        if (milestones.Count == 0)
        {
            return null;
        }

        if (requireConfirmation && !inputReader.ReadConfirmation(
            "Associate this quest with a milestone?"
        ))
        {
            return null;
        }

        return inputReader.ReadSelection(
            "Select the milestone:",
            milestones,
            milestone => $"{milestone.Order}. {milestone.Title} — {milestone.Status}"
        );
    }

    private void CompleteQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Concluir a quest '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Conclusão da quest cancelada."
            );
            return;
        }

        QuestCompletionResult result =
            questWorkflowService.CompleteQuest(quest.Id);

        ConsoleHelper.ShowSuccess(
            "Quest concluída com sucesso."
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

        if (result.ProjectCompleted)
        {
            ConsoleHelper.ShowSuccess(
                "All valid quests and milestones were completed. " +
                "The project was completed automatically."
            );
        }
    }

    private void ArchiveQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Arquivar a quest '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Arquivamento cancelado."
            );
            return;
        }

        questService.ArchiveQuest(quest);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Quest arquivada com sucesso."
        );
    }

    private bool DeleteQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Excluir permanentemente '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Exclusão cancelada."
            );
            return false;
        }

        bool deleted = questService.DeleteQuest(quest.Id);

        if (!deleted)
        {
            ConsoleHelper.ShowError(
                "Não foi possível excluir a quest."
            );
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Quest excluída com sucesso."
        );
        return true;
    }

    private Project? SelectOptionalProject()
    {
        List<Project> projects = projectService
            .GetAllProjects()
            .Where(project =>
                project.Status is ProjectStatus.Created or
                    ProjectStatus.Active
            )
            .ToList();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum projeto disponível. " +
                "A quest será independente."
            );
            return null;
        }

        if (!inputReader.ReadConfirmation(
            "Deseja associar esta quest a um projeto?"
        ))
        {
            return null;
        }

        return inputReader.ReadSelection(
            "Selecione o projeto:",
            projects,
            project => $"{project.Name} — {project.Status}"
        );
    }

    private Quest SelectQuest(
        string prompt,
        IEnumerable<QuestModel> quests
    )
    {
        return inputReader.ReadSelection(
            prompt,
            quests,
            quest =>
                $"{quest.Title} — {quest.Status} — " +
                GetProjectName(quest.ProjectId)
        );
    }

    private QuestCard BuildQuestCard(Quest quest)
    {
        return new QuestCard(
            quest,
            GetProjectName(quest.ProjectId)
        );
    }

    private string GetProjectName(int? projectId)
    {
        if (projectId is null)
        {
            return "Independent";
        }

        return projectService.GetProjectById(
            projectId.Value
        )?.Name ?? "Project not found";
    }

    private void ShowProjectProgress(int? projectId)
    {
        if (projectId is null)
        {
            return;
        }

        Project? project = projectService.GetProjectById(
            projectId.Value
        );

        if (project is null)
        {
            return;
        }

        decimal progress = projectService.CalculateProgress(
            project,
            questService.GetAllQuests()
        );

        AnsiConsole.MarkupLine(
            $"[grey]Progresso de " +
            $"{Markup.Escape(project.Name)}:[/] " +
            $"[green]{progress:0.##}%[/]"
        );
    }
}
