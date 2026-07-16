using LevelUp.Domain.Milestones;
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
            ConsoleHelper.ShowHeader("Painel de Missões");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Nova missão",
                    "Abrir missão",
                    "Listar missões",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Nova missão":
                    CreateQuest();
                    inputReader.WaitForContinue();
                    break;

                case "Abrir missão":
                    OpenQuest();
                    break;

                case "Listar missões":
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
        ConsoleHelper.ShowHeader("Nova missão");
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel("Título:");
            string description = inputReader.ReadRequiredStringOrCancel(
                "Descrição:"
            );
            Project? project = selectionFlow.SelectOptionalProjectForCreation();
            Milestone? milestone = project is null
                ? null
                : selectionFlow.SelectOptionalMilestoneForCreation(project);

            Quest quest = questService.CreateQuest(
                title,
                description,
                project
            );

            if (milestone is not null)
            {
                questService.AssignQuestToMilestone(quest, milestone);
            }

            questService.ActivateQuest(quest);
            gameStateService.Save();

            ConsoleHelper.ShowSuccess(
                "Missão cadastrada com sucesso."
            );
            AnsiConsole.WriteLine();
            AnsiConsole.Write(BuildQuestCard(quest).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation(
                "Criação da missão cancelada."
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
                "Nenhuma missão foi cadastrada."
            );
            inputReader.WaitForContinue();
            return;
        }

        Quest selectedQuest = selectionFlow.SelectQuest(
            "Selecione uma missão:",
            quests
        );

        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Missão");
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

                case "Alterar capítulo":
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
            actions.Add("Alterar capítulo");
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
        ConsoleHelper.ShowHeader("Painel de Missões");

        IReadOnlyList<QuestModel> quests =
            questService.GetAllQuests();

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhuma missão foi cadastrada."
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
            "Missão atualizada com sucesso."
        );
    }

    private void ChangeQuestProject(Quest quest)
    {
        string currentProject = selectionFlow.GetProjectName(
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
            "Missão associada ao projeto com sucesso."
        );
        return true;
    }

    private bool RemoveQuestFromProject(Quest quest)
    {
        if (quest.ProjectId is null)
        {
            ConsoleHelper.ShowInformation(
                "A missão já é independente."
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
                "Associe a missão a um projeto antes de selecionar um capítulo."
            );
            return;
        }

        Project? project = projectService.GetProjectById(quest.ProjectId.Value);
        if (project is null)
        {
            return;
        }

        string option = inputReader.ReadSelection(
            "Escolha uma ação:",
            new[] { "Associar a um capítulo", "Remover capítulo", "Cancelar" },
            choice => choice
        );

        if (option == "Associar a um capítulo")
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
                ConsoleHelper.ShowSuccess("Associação com o capítulo atualizada.");
            }
        }
        else if (option == "Remover capítulo" && quest.MilestoneId is not null)
        {
            questService.RemoveQuestFromMilestone(quest);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Associação com o capítulo removida.");
        }
    }

    private void CompleteQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Concluir a missão '{quest.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation(
                "Conclusão da missão cancelada."
            );
            return;
        }

        QuestCompletionResult result =
            questWorkflowService.CompleteQuest(quest.Id);

        ConsoleHelper.ShowSuccess(
            "Missão concluída com sucesso."
        );

        ShowProjectProgress(result.Quest.ProjectId);

        if (result.MilestoneCompleted)
        {
            ConsoleHelper.ShowSuccess("O capítulo foi concluído automaticamente.");
        }

        if (result.UnlockedBoss is not null)
        {
            ConsoleHelper.ShowSuccess(
                $"Chefe desbloqueado: {result.UnlockedBoss.Name}."
            );
        }

        if (result.ActivatedMilestone is not null)
        {
            ConsoleHelper.ShowSuccess(
                $"Próximo capítulo ativado: {result.ActivatedMilestone.Title}."
            );
        }

        if (result.ProjectCompleted)
        {
            ConsoleHelper.ShowSuccess(
                "Todas as missões e capítulos válidos foram concluídos. " +
                "O projeto foi concluído automaticamente."
            );
        }
    }

    private void ArchiveQuest(Quest quest)
    {
        if (!inputReader.ReadConfirmation(
            $"Arquivar a missão '{quest.Title}'?"
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
            "Missão arquivada com sucesso."
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
                "Não foi possível excluir a missão."
            );
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess(
            "Missão excluída com sucesso."
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
            $"[grey]Progresso de " +
            $"{Markup.Escape(project.Name)}:[/] " +
            $"[green]{progress:0.##}%[/]"
        );
    }


}
