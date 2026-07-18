namespace LevelUp.UI;

public sealed class InventoryScreen
{
    private readonly InputReader inputReader;
    private readonly LibraryScreen libraryScreen;
    private readonly WalletScreen walletScreen;

    public InventoryScreen(InputReader inputReader, LibraryScreen libraryScreen, WalletScreen walletScreen)
    {
        this.inputReader = inputReader;
        this.libraryScreen = libraryScreen;
        this.walletScreen = walletScreen;
    }

    public void Show()
    {
        bool running = true;
        while (running)
        {
            ConsoleHelper.ShowHeader("Inventory");
            string option = inputReader.ReadSelection("Choose an option:", new[] { "Library", "Wallet", "Back" }, choice => choice);
            switch (option)
            {
                case "Library": libraryScreen.Show(); break;
                case "Wallet": walletScreen.Show(); break;
                case "Back": running = false; break;
            }
        }
    }
}
