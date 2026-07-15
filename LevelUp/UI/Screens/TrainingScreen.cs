using LevelUp.Domain;
using LevelUp.Domain.Attributes;
using LevelUp.Domain.Habits;
using LevelUp.Services.Character;
using LevelUp.Services.Habits;
using LevelUp.Services.Persistence;
using LevelUp.UI.Components.Training;
using Spectre.Console;
using CharacterModel = LevelUp.Domain.Character.Character;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.UI;

public class TrainingScreen
{
    private readonly HabitService habitService;
    private readonly CharacterService characterService;
    private readonly AttributeService attributeService;
    private readonly SaveService saveService;
    private readonly InputReader inputReader;
    private readonly CharacterModel character;
    private readonly ProjectService projectService;
    private readonly QuestService questService;

    public TrainingScreen(
    HabitService habitService,
    CharacterService characterService,
    AttributeService attributeService,
    SaveService saveService,
    InputReader inputReader,
    CharacterModel character,
    ProjectService projectService,
    QuestService questService)
    {
        this.habitService = habitService;
        this.characterService = characterService;
        this.attributeService = attributeService;
        this.saveService = saveService;
        this.inputReader = inputReader;
        this.character = character;
        this.projectService = projectService;
        this.questService = questService;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Training");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                "Cadastrar treinamento",
                "Listar treinamentos",
                "Concluir treinamento",
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
            case "Cadastrar treinamento":
                CreateTraining();
                inputReader.WaitForContinue();
                return true;

            case "Listar treinamentos":
                ListTrainings();
                inputReader.WaitForContinue();
                return true;

            case "Concluir treinamento":
                CompleteTraining();
                inputReader.WaitForContinue();
                return true;

            case "Voltar":
                return false;

            default:
                return true;
        }
    }


    private void CreateTraining()
    {
        Console.Clear();

        ConsoleHelper.ShowHeader("Cadastrar treinamento");

        string title = inputReader.ReadRequiredString(
            "Título: "
        );

        string description = inputReader.ReadRequiredString(
            "Descrição: "
        );

        int durationInMinutes =
            inputReader.ReadPositiveInteger(
                "Duração em minutos: "
            );

        AttributeType attributeType = SelectAttribute();

        Habit habit = habitService.CreateHabit(
            title,
            description,
            durationInMinutes,
            attributeType
        );

        SaveGame();

        ConsoleHelper.ShowSuccess(
            "Treinamento cadastrado com sucesso."
        );

        AnsiConsole.WriteLine();

        TrainingCreatedCard createdCard = new(habit);

        AnsiConsole.Write(createdCard.Build());
    }

    private AttributeType SelectAttribute()
    {
        return inputReader.ReadSelection(
            "Escolha o atributo:",
            Enum.GetValues<AttributeType>(),
            attribute => attribute.ToString()
        );
    }

    private void ListTrainings()
    {
        ConsoleHelper.ShowHeader("Trainings");

        List<Habit> habits = habitService.GetAllHabits();

        if (habits.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum treinamento foi cadastrado."
            );

            return;
        }

        TrainingTable trainingTable = new(habits);

        AnsiConsole.Write(trainingTable.Build());
    }

    private void CompleteTraining()
    {
        ConsoleHelper.ShowHeader("Concluir treinamento");

        List<Habit> habits =
            habitService.GetAllHabits();

        if (habits.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhum treinamento foi cadastrado."
            );

            return;
        }



        Habit selectedHabit = inputReader.ReadSelection(
            "Selecione o treinamento concluído:",
            habits,
            habit =>
                $"{habit.Title} — " +
                $"{habit.ExperienceReward:0.##} XP — " +
                $"{habit.AttributeType}"
        );

        bool confirmed = inputReader.ReadConfirmation(
            $"Concluir o treinamento '{selectedHabit.Title}'?"
        );

        if (!confirmed)
        {
            ConsoleHelper.ShowInformation(
                "Conclusão do treinamento cancelada."
            );

            return;
        }

        decimal experienceEarned =
            habitService.CompleteHabit(selectedHabit);

        characterService.AddExperience(
            character,
            experienceEarned
        );

        attributeService.AddExperience(
            character.Attributes,
            selectedHabit.AttributeType,
            selectedHabit.AttributeExperienceReward
        );

        SaveGame();

        ConsoleHelper.ShowSuccess(
            "Treinamento concluíd0o com sucesso."
        );

        AnsiConsole.WriteLine();

        TrainingResultCard resultCard = new(
            selectedHabit,
            character,
            experienceEarned
        );

        AnsiConsole.Write(resultCard.Build());

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