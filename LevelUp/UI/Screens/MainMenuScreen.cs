using LevelUp.Services.Persistence;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public sealed class MainMenuScreen
{
    private readonly InputReader inputReader;
    private readonly CharacterScreen characterScreen;
    private readonly DiaryScreen diaryScreen;
    private readonly InventoryScreen inventoryScreen;
    private readonly SettingsScreen settingsScreen;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public MainMenuScreen(
        InputReader inputReader,
        CharacterScreen characterScreen,
        DiaryScreen diaryScreen,
        InventoryScreen inventoryScreen,
        SettingsScreen settingsScreen,
        CharacterModel character,
        GameStateService gameStateService
    )
    {
        this.inputReader = inputReader;
        this.characterScreen = characterScreen;
        this.diaryScreen = diaryScreen;
        this.inventoryScreen = inventoryScreen;
        this.settingsScreen = settingsScreen;
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
                    "Diário",
                    "Inventário",
                    "Configurações",
                    "Salvar jogo",
                    "Sair"
                },
                choice => choice
            );

            try
            {
                switch (option)
                {
                    case "Personagem": characterScreen.Show(character); break;
                    case "Diário": diaryScreen.Show(); break;
                    case "Inventário": inventoryScreen.Show(); break;
                    case "Configurações": settingsScreen.Show(); break;
                    case "Salvar jogo":
                        gameStateService.Save();
                        ConsoleHelper.ShowSuccess("Jogo salvo com sucesso.");
                        inputReader.WaitForContinue();
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
                ConsoleHelper.ShowError($"Ocorreu um erro de armazenamento: {exception.Message}");
                inputReader.WaitForContinue();
            }
        }

        ConsoleHelper.ShowHeader("Até breve");
    }
}
