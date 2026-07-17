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
            ConsoleHelper.ShowHeader("Inventário");
            string option = inputReader.ReadSelection("Escolha uma opção:", new[] { "Biblioteca", "Carteira", "Voltar" }, choice => choice);
            switch (option)
            {
                case "Biblioteca": libraryScreen.Show(); break;
                case "Carteira": walletScreen.Show(); break;
                case "Voltar": running = false; break;
            }
        }
    }
}
