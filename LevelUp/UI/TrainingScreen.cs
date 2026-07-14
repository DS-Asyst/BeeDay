using LevelUp.Models;
using LevelUp.Services;

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
            DisplayMenu();

            int option = inputReader.ReadOption(
                "Escolha uma opção: ",
                0,
                3
            );

            running = HandleOption(option);
        }
    }

    private bool HandleOption(int option)
    {
        switch (option)
        {
            case 1:
                CreateTraining();
                inputReader.WaitForContinue();
                return true;

            case 2:
                ListTrainings();
                inputReader.WaitForContinue();
                return true;

            case 3:
                CompleteTraining();
                inputReader.WaitForContinue();
                return true;

            case 0:
                return false;

            default:
                return true;
        }
    }

    private static void DisplayMenu()
    {
        ConsoleHelper.ShowHeader("Training");

        Console.WriteLine("1 - Cadastrar treinamento");
        Console.WriteLine("2 - Listar treinamentos");
        Console.WriteLine("3 - Concluir treinamento");
        Console.WriteLine("0 - Voltar");
        Console.WriteLine();
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
        Console.WriteLine();
        Console.WriteLine("Escolha o atributo:");
        Console.WriteLine("1 - Strength");
        Console.WriteLine("2 - Intelligence");
        Console.WriteLine("3 - Vitality");
        Console.WriteLine("4 - Agility");
        Console.WriteLine("5 - Luck");
        Console.WriteLine("6 - Dexterity");
        Console.WriteLine();

        int option = inputReader.ReadOption(
            "Opção: ",
            1,
            6
        );

        return (AttributeType)option;
    }

    private void ListTrainings()
    {
        Console.Clear();

        ConsoleHelper.ShowHeader("Treinamentos");

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
            Console.WriteLine($"ID: {habit.Id}");
            Console.WriteLine($"Título: {habit.Title}");
            Console.WriteLine(
                $"Descrição: {habit.Description}"
            );
            Console.WriteLine(
                $"Atributo: {habit.AttributeType}"
            );
            Console.WriteLine(
                $"Duração: {habit.DurationInMinutes} minutos"
            );
            Console.WriteLine(
                $"Recompensa: {habit.ExperienceReward} XP"
            );
            Console.WriteLine(
                $"XP de atributo: " +
                $"{habit.AttributeExperienceReward}"
            );
            Console.WriteLine(
                $"Conclusões: {habit.TimesCompleted}"
            );
            Console.WriteLine(
                "--------------------------------"
            );
        }
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