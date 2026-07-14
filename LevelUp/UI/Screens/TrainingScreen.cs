using LevelUp.Models;
using LevelUp.Services;
using LevelUp.UI.Components.Training;
using Spectre.Console;

namespace LevelUp.UI;

public class TrainingScreen
{
    private readonly HabitService habitService;
    private readonly CharacterService characterService;
    private readonly AttributeService attributeService;
    private readonly SaveService saveService;
    private readonly InputReader inputReader;
    private readonly Character character;

    public TrainingScreen(
        HabitService habitService,
        CharacterService characterService,
        AttributeService attributeService,
        SaveService saveService,
        InputReader inputReader,
        Character character)
    {
        this.habitService = habitService;
        this.characterService = characterService;
        this.attributeService = attributeService;
        this.saveService = saveService;
        this.inputReader = inputReader;
        this.character = character;
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

        Console.WriteLine($"ID: {habit.Id}");
        Console.WriteLine($"Título: {habit.Title}");
        Console.WriteLine(
            $"Recompensa: {habit.ExperienceReward} XP"
        );
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
        Console.Clear();
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



        foreach (Habit habit in habits)
        {
            Console.WriteLine(
                $"{habit.Id} - {habit.Title} " +
                $"({habit.ExperienceReward} XP)"
            );
        }

        Console.WriteLine();

        int habitId = inputReader.ReadPositiveInteger(
            "Digite o ID do treinamento concluído: "
        );

        Habit? selectedHabit = habits.FirstOrDefault(
            habit => habit.Id == habitId
        );

        if (selectedHabit is null)
        {
            ConsoleHelper.ShowError(
                "Treinamento não encontrado."
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
        "Treinamento concluído com sucesso."
        );

        Console.WriteLine();
        Console.WriteLine(
            "Treinamento concluído com sucesso."
        );
        Console.WriteLine(
            $"Treinamento: {selectedHabit.Title}"
        );
        Console.WriteLine(
            $"XP recebida: {experienceEarned}"
        );
        Console.WriteLine(
            $"Conclusões: {selectedHabit.TimesCompleted}"
        );

        Console.WriteLine();
        Console.WriteLine(
            $"Nível atual: {character.Level}"
        );
        Console.WriteLine(
            $"Experiência: {character.Experience}/" +
            $"{character.ExperienceToNextLevel}"
        );
    }

    private void SaveGame()
    {
        GameData gameData = new()
        {
            Character = character,
            Habits = habitService.GetAllHabits()
        };

        saveService.SaveGame(gameData);
    }
}