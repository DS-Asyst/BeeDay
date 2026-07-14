using LevelUp.Services.Habits;
using LevelUp.Services.Persistence;
using LevelUp.Domain;
using LevelUp.Domain.Habits;
using CharacterModel = LevelUp.Domain.Character.Character;


namespace LevelUp.UI;

public class MainMenuScreen
{
    private readonly InputReader inputReader;
    private readonly CharacterScreen characterScreen;
    private readonly TrainingScreen trainingScreen;
    private readonly QuestScreen questScreen;
    private readonly ProjectScreen projectScreen;
    private readonly GoldScreen goldScreen;
    private readonly CharacterModel character;
    private readonly HabitService habitService;
    private readonly SaveService saveService;

    public MainMenuScreen(
        InputReader inputReader,
        CharacterScreen characterScreen,
        TrainingScreen trainingScreen,
        QuestScreen questScreen,
        ProjectScreen projectScreen,
        GoldScreen goldScreen,
        CharacterModel character,
        HabitService habitService,
        SaveService saveService)
    {
        this.inputReader = inputReader;
        this.characterScreen = characterScreen;
        this.trainingScreen = trainingScreen;
        this.questScreen = questScreen;
        this.projectScreen = projectScreen;
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
            ConsoleHelper.ShowHeader("Level Up");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                "Character",
                "Training",
                "Quests",
                "Projects",
                "Gold",
                "Exit"
                },
                choice => choice
            );

            switch (option)
            {
                case "Character":
                    characterScreen.Show(character);
                    inputReader.WaitForContinue();
                    break;

                case "Training":
                    trainingScreen.Show();
                    break;

                case "Quests":
                    questScreen.Show();
                    break;

                case "Projects":
                    projectScreen.Show();
                    break;

                case "Gold":
                    goldScreen.Show();
                    break;

                case "Exit":
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

    private static void ShowExitMessage()
    {
        ConsoleHelper.ShowHeader("See you soon");
    }
}