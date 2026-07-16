using LevelUp.Services.Persistence;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public class MainMenuScreen
{
    private readonly InputReader inputReader;
    private readonly CharacterScreen characterScreen;
    private readonly TrainingScreen trainingScreen;
    private readonly QuestScreen questScreen;
    private readonly ProjectScreen projectScreen;
    private readonly MilestoneScreen milestoneScreen;
    private readonly GoldScreen goldScreen;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public MainMenuScreen(
        InputReader inputReader,
        CharacterScreen characterScreen,
        TrainingScreen trainingScreen,
        QuestScreen questScreen,
        ProjectScreen projectScreen,
        MilestoneScreen milestoneScreen,
        GoldScreen goldScreen,
        CharacterModel character,
        GameStateService gameStateService
    )
    {
        this.inputReader = inputReader;
        this.characterScreen = characterScreen;
        this.trainingScreen = trainingScreen;
        this.questScreen = questScreen;
        this.projectScreen = projectScreen;
        this.milestoneScreen = milestoneScreen;
        this.goldScreen = goldScreen;
        this.character = character;
        this.gameStateService = gameStateService;
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
                    "Milestones",
                    "Gold",
                    "Exit"
                },
                choice => choice
            );

            try
            {
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

                    case "Milestones":
                        milestoneScreen.Show();
                        break;

                    case "Gold":
                        goldScreen.Show();
                        break;

                    case "Exit":
                        gameStateService.Save();
                        running = false;
                        break;
                }
            }
            catch (InvalidOperationException exception)
            {
                ConsoleHelper.ShowError(exception.Message);
                inputReader.WaitForContinue();
            }
            catch (IOException exception)
            {
                ConsoleHelper.ShowError(
                    $"A storage error occurred: {exception.Message}"
                );
                inputReader.WaitForContinue();
            }
        }

        ConsoleHelper.ShowHeader("See you soon");
    }
}
