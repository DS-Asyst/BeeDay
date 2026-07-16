using LevelUp.Services.Persistence;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public sealed class MainMenuScreen
{
    private readonly InputReader inputReader;
    private readonly DashboardScreen dashboardScreen;
    private readonly CharacterScreen characterScreen;
    private readonly DiaryScreen diaryScreen;
    private readonly LibraryScreen libraryScreen;
    private readonly BackpackScreen backpackScreen;
    private readonly SettingsScreen settingsScreen;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public MainMenuScreen(
        InputReader inputReader,
        DashboardScreen dashboardScreen,
        CharacterScreen characterScreen,
        DiaryScreen diaryScreen,
        LibraryScreen libraryScreen,
        BackpackScreen backpackScreen,
        SettingsScreen settingsScreen,
        CharacterModel character,
        GameStateService gameStateService
    )
    {
        this.inputReader = inputReader;
        this.dashboardScreen = dashboardScreen;
        this.characterScreen = characterScreen;
        this.diaryScreen = diaryScreen;
        this.libraryScreen = libraryScreen;
        this.backpackScreen = backpackScreen;
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
                    "Visão geral",
                    "Personagem",
                    "Diário",
                    "Biblioteca",
                    "Mochila",
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
                    case "Visão geral": dashboardScreen.Show(); break;
                    case "Personagem": characterScreen.Show(character); break;
                    case "Diário": diaryScreen.Show(); break;
                    case "Biblioteca": libraryScreen.Show(); break;
                    case "Mochila": backpackScreen.Show(); break;
                    case "Configurações": settingsScreen.Show(); break;
                    case "Salvar jogo":
                        gameStateService.Save();
                        ConsoleHelper.ShowSuccess("Jogo salvo com sucesso.");
                        inputReader.WaitForContinue();
                        break;
                    case "Sair": gameStateService.Save(); running = false; break;
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
