using LevelUp.Services.Persistence;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI;

public sealed class MainMenuScreen
{
    private readonly InputReader inputReader;
    private readonly CharacterScreen characterScreen;
    private readonly DiaryScreen diaryScreen;
    private readonly LibraryScreen libraryScreen;
    private readonly BackpackScreen backpackScreen;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public MainMenuScreen(
        InputReader inputReader,
        CharacterScreen characterScreen,
        DiaryScreen diaryScreen,
        LibraryScreen libraryScreen,
        BackpackScreen backpackScreen,
        CharacterModel character,
        GameStateService gameStateService
    )
    {
        this.inputReader = inputReader;
        this.characterScreen = characterScreen;
        this.diaryScreen = diaryScreen;
        this.libraryScreen = libraryScreen;
        this.backpackScreen = backpackScreen;
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
                    "Biblioteca",
                    "Mochila",
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

                    case "Diário":
                        diaryScreen.Show();
                        break;

                    case "Biblioteca":
                        libraryScreen.Show();
                        break;

                    case "Mochila":
                        backpackScreen.Show();
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
