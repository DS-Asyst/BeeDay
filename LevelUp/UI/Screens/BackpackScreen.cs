namespace LevelUp.UI;

public sealed class BackpackScreen
{
    private readonly InputReader inputReader;
    private readonly WalletScreen walletScreen;

    public BackpackScreen(
        InputReader inputReader,
        WalletScreen walletScreen
    )
    {
        this.inputReader = inputReader;
        this.walletScreen = walletScreen;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Mochila");

            string option = inputReader.ReadSelection(
                "Escolha um item:",
                new[]
                {
                    "Carteira",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Carteira":
                    walletScreen.Show();
                    break;

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }
}
