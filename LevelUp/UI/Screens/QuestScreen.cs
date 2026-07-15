using LevelUp.Domain;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Habits;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using Spectre.Console;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public sealed class QuestScreen
{
    private readonly QuestService questService;
    private readonly ProjectService projectService;
    private readonly HabitService habitService;
    private readonly SaveService saveService;
    private readonly InputReader inputReader;
    private readonly CharacterModel character;

    public QuestScreen(
        QuestService questService,
        ProjectService projectService,
        HabitService habitService,
        SaveService saveService,
        InputReader inputReader,
        CharacterModel character)
    {
        this.questService = questService;
        this.projectService = projectService;
        this.habitService = habitService;
        this.saveService = saveService;
        this.inputReader = inputReader;
        this.character = character;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Quests");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Cadastrar quest",
                    "Listar quests",
                    "Concluir quest",
                    "Voltar"
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
            case "Cadastrar quest":
                CreateQuest();
                inputReader.WaitForContinue();
                return true;

            case "Listar quests":
                ListQuests();
                inputReader.WaitForContinue();
                return true;

            case "Concluir quest":
                CompleteQuest();
                inputReader.WaitForContinue();
                return true;

            case "Voltar":
                return false;

            default:
                return true;
        }
    }

    private void CreateQuest()
    {
        ConsoleHelper.ShowHeader("Cadastrar quest");

        string title = inputReader.ReadRequiredString(
            "Título:"
        );

        string description = inputReader.ReadRequiredString(
            "Descrição:"
        );

        Project? project = SelectOptionalProject();

        Quest quest = questService.CreateQuest(
            title,
            description,
            project
        );

        questService.ActivateQuest(quest);

        SaveGame();

        ConsoleHelper.ShowSuccess(
            "Quest cadastrada com sucesso."
        );

        AnsiConsole.MarkupLine(
            $"[grey]ID:[/] {quest.Id}"
        );

        AnsiConsole.MarkupLine(
            $"[grey]Título:[/] {Markup.Escape(quest.Title)}"
        );

        string projectText = project is null
            ? "Independent"
            : Markup.Escape(project.Name);

        AnsiConsole.MarkupLine(
            $"[grey]Projeto:[/] {projectText}"
        );
    }

    private Project? SelectOptionalProject()
    {
        IReadOnlyList<Project> projects =
            projectService.GetAllProjects();

        if (projects.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum projeto cadastrado. " +
                "A quest será independente."
            );

            return null;
        }

        bool associateWithProject =
            inputReader.ReadConfirmation(
                "Deseja associar esta quest a um projeto?"
            );

        if (!associateWithProject)
        {
            return null;
        }

        return inputReader.ReadSelection(
            "Selecione o projeto:",
            projects,
            project =>
                $"{project.Name} — {project.Status}"
        );
    }

    private void ListQuests()
    {
        ConsoleHelper.ShowHeader("Quests");

        IReadOnlyList<Quest> quests =
            questService.GetAllQuests();

        if (quests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhuma quest foi cadastrada."
            );

            return;
        }

        Table table = new();

        table.AddColumn("ID");
        table.AddColumn("Title");
        table.AddColumn("Status");
        table.AddColumn("Project");

        foreach (Quest quest in quests)
        {
            string projectName =
                GetProjectName(quest.ProjectId);

            table.AddRow(
                quest.Id.ToString(),
                Markup.Escape(quest.Title),
                quest.Status.ToString(),
                Markup.Escape(projectName)
            );
        }

        AnsiConsole.Write(table);
    }

    private void CompleteQuest()
    {
        ConsoleHelper.ShowHeader("Concluir quest");

        List<Quest> availableQuests = questService
            .GetAllQuests()
            .Where(quest =>
                quest.Status == QuestStatus.Active
            )
            .ToList();

        if (availableQuests.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Não existem quests ativas para concluir."
            );

            return;
        }

        Quest selectedQuest = inputReader.ReadSelection(
            "Selecione a quest concluída:",
            availableQuests,
            quest =>
            {
                string projectName =
                    GetProjectName(quest.ProjectId);

                return $"{quest.Title} — {projectName}";
            }
        );

        bool confirmed = inputReader.ReadConfirmation(
            $"Concluir a quest '{selectedQuest.Title}'?"
        );

        if (!confirmed)
        {
            ConsoleHelper.ShowInformation(
                "Conclusão da quest cancelada."
            );

            return;
        }

        questService.CompleteQuest(selectedQuest);

        SaveGame();

        ConsoleHelper.ShowSuccess(
            "Quest concluída com sucesso."
        );

        ShowProjectProgress(selectedQuest.ProjectId);
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

    private string GetProjectName(int? projectId)
    {
        if (projectId is null)
        {
            return "Independent";
        }

        Project? project =
            projectService.GetProjectById(
                projectId.Value
            );

        return project?.Name
            ?? "Project not found";
    }

    private void SaveGame()
    {
        GameData gameData = new()
        {
            Character = character,

            Habits = habitService
                .GetAllHabits()
                .ToList(),

            Projects = projectService
                .GetAllProjects()
                .ToList(),

            Quests = questService
                .GetAllQuests()
                .ToList()
        };

        saveService.SaveGame(gameData);
    }
}