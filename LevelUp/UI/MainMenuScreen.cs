using LevelUp.Models;
using LevelUp.Services;

namespace LevelUp.UI;

public class MainMenuScreen
{
    private readonly InputReader inputReader;
    private readonly CharacterScreen characterScreen;
    private readonly TrainingScreen trainingScreen;
    private readonly QuestScreen questScreen;
    private readonly BossScreen bossScreen;
    private readonly GoldScreen goldScreen;
    private readonly Character character;
    private readonly HabitService habitService;
    private readonly SaveService saveService;

    public MainMenuScreen(
        InputReader inputReader,
        CharacterScreen characterScreen,
        TrainingScreen trainingScreen,
        QuestScreen questScreen,
        BossScreen bossScreen,
        GoldScreen goldScreen,
        Character character,
        HabitService habitService,
        SaveService saveService)
    {
        this.inputReader = inputReader;
        this.characterScreen = characterScreen;
        this.trainingScreen = trainingScreen;
        this.questScreen = questScreen;
        this.bossScreen = bossScreen;
        this.goldScreen = goldScreen;
        this.character = character;
        this.habitService = habitService;
        this.saveService = saveService;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();

            DisplayMenu();

            int option = inputReader.ReadOption(
                "Escolha uma opção: ",
                0,
                5
            );

            switch (option)
            {
                case 1:
                    characterScreen.Show(character);
                    inputReader.WaitForContinue();
                    break;

                case 2:
                    trainingScreen.Show();
                    break;

                case 3:
                    questScreen.Show();
                    break;

                case 4:
                    bossScreen.Show();
                    break;

                case 5:
                    goldScreen.Show();
                    break;

                case 0:
                    SaveGame();
                    running = false;
                    break;
            }
        }

        ShowExitMessage();
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

    private static void DisplayMenu()
    {
        ConsoleHelper.ShowHeader("Level Up");

        Console.WriteLine("1 - Character");
        Console.WriteLine("2 - Training");
        Console.WriteLine("3 - Quests");
        Console.WriteLine("4 - Bosses");
        Console.WriteLine("5 - Gold");
        Console.WriteLine("0 - Exit");
        Console.WriteLine();
    }

    private static void ShowExitMessage()
    {
        ConsoleHelper.ShowHeader("See you soon");
    }
}