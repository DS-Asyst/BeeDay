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
                    "Personagem",
                    "Treinamentos",
                    "Missões",
                    "Projetos",
                    "Capítulos",
                    "Finanças",
                    "Sair"
                },
                choice => choice
            );

            try
            {
                switch (option)
                {
                    case "Personagem":
                        characterScreen.Show(character);
                        inputReader.WaitForContinue();
                        break;

                    case "Treinamentos":
                        trainingScreen.Show();
                        break;

                    case "Missões":
                        questScreen.Show();
                        break;

                    case "Projetos":
                        projectScreen.Show();
                        break;

                    case "Capítulos":
                        milestoneScreen.Show();
                        break;

                    case "Finanças":
                        goldScreen.Show();
                        break;

                    case "Sair":
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
                    $"Ocorreu um erro de armazenamento: {exception.Message}"
                );
                inputReader.WaitForContinue();
            }
        }

        ConsoleHelper.ShowHeader("Até breve");
    }
}
