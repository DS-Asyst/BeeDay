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
                "Choose an option:",
                new[]
                {
                    "Character",
                    "Journal",
                    "Inventory",
                    "Settings",
                    "Save Game",
                    "Exit"
                },
                choice => choice
            );

            try
            {
                switch (option)
                {
                    case "Character": characterScreen.Show(character); break;
                    case "Journal": diaryScreen.Show(); break;
                    case "Inventory": inventoryScreen.Show(); break;
                    case "Settings": settingsScreen.Show(); break;
                    case "Save Game":
                        gameStateService.Save();
                        ConsoleHelper.ShowSuccess("Game saved successfully.");
                        inputReader.WaitForContinue();
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
                ConsoleHelper.ShowError($"A storage error occurred: {exception.Message}");
                inputReader.WaitForContinue();
            }
        }

        ConsoleHelper.ShowHeader("See you soon");
    }
}
