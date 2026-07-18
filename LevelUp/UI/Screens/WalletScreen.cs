using LevelUp.Domain.Wallet;
using LevelUp.Services.Persistence;
using LevelUp.Services.Wallet;
using LevelUp.UI.Components.Wallet;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI;

public sealed class WalletScreen
{
    private readonly WalletService walletService;
    private readonly GameStateService gameStateService;
    private readonly InputReader inputReader;

    public WalletScreen(
        WalletService walletService,
        GameStateService gameStateService,
        InputReader inputReader
    )
    {
        this.walletService = walletService;
        this.gameStateService = gameStateService;
        this.inputReader = inputReader;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Wallet");
            ShowBalance();

            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "Record Transaction",
                    "Open Transaction",
                    "View History",
                    "Monthly Summary",
                    "Manage Tags",
                    "Back"
                },
                choice => choice
            );

            switch (option)
            {
                case "Record Transaction":
                    CreateTransaction();
                    inputReader.WaitForContinue();
                    break;
                case "Open Transaction":
                    OpenTransaction();
                    break;
                case "View History":
                    ShowHistory();
                    inputReader.WaitForContinue();
                    break;
                case "Monthly Summary":
                    ShowMonthlySummary();
                    inputReader.WaitForContinue();
                    break;
                case "Manage Tags":
                    ManageTags();
                    break;
                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private void CreateTransaction()
    {
        ConsoleHelper.ShowHeader("New Transaction");
        inputReader.ShowCancellationHint();

        try
        {
            decimal amount = inputReader.ReadDecimalOrCancel(
                "Amount (positive for credit, negative for debit):"
            );

            if (amount == 0)
            {
                ConsoleHelper.ShowError("The amount cannot be zero.");
                return;
            }

            string description = inputReader.ReadRequiredStringOrCancel("Description:");
            WalletTag tag = SelectTagForTransaction();

            if (inputReader.ReadDecision("Confirm transaction?") != PromptDecision.Yes)
            {
                ConsoleHelper.ShowInformation("Transaction cancelled.");
                return;
            }

            walletService.AddTransaction(amount, description, tag);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Transaction recorded successfully.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Transaction cancelled.");
        }
    }

    private WalletTag SelectTagForTransaction()
    {
        IReadOnlyList<WalletTag> tags = walletService.GetAllTags();
        if (tags.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "No tags have been created. Create a tag to continue."
            );
            string name = inputReader.ReadRequiredStringOrCancel("New tag name:");
            WalletTag created = walletService.CreateTag(name);
            gameStateService.Save();
            return created;
        }

        return inputReader.ReadSelection(
            "Select a tag:",
            tags,
            tag => tag.Name
        );
    }

    private void ManageTags()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Tags da Wallet");
            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "Create Tag",
                    "Edit Tag",
                    "Delete Tag",
                    "List Tags",
                    "Back"
                },
                choice => choice
            );

            switch (option)
            {
                case "Create Tag":
                    CreateTag();
                    inputReader.WaitForContinue();
                    break;
                case "Edit Tag":
                    EditTag();
                    inputReader.WaitForContinue();
                    break;
                case "Delete Tag":
                    DeleteTag();
                    inputReader.WaitForContinue();
                    break;
                case "List Tags":
                    ListTags();
                    inputReader.WaitForContinue();
                    break;
                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private void CreateTag()
    {
        inputReader.ShowCancellationHint();
        try
        {
            string name = inputReader.ReadRequiredStringOrCancel("Tag name:");
            walletService.CreateTag(name);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Tag created successfully.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Tag creation cancelled.");
        }
    }

    private void EditTag()
    {
        WalletTag? tag = TrySelectTag();
        if (tag is null)
        {
            return;
        }

        inputReader.ShowCancellationHint();
        try
        {
            string name = inputReader.ReadRequiredStringOrCancel("New name:");
            walletService.UpdateTag(tag, name);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Tag updated successfully.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Tag edit cancelled.");
        }
    }

    private void DeleteTag()
    {
        WalletTag? tag = TrySelectTag();
        if (tag is null)
        {
            return;
        }

        if (!inputReader.ReadConfirmation($"Delete the tag '{tag.Name}'?"))
        {
            ConsoleHelper.ShowInformation("Deletion cancelled.");
            return;
        }

        walletService.DeleteTag(tag.Id);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Tag deleted successfully.");
    }

    private void ListTags()
    {
        IReadOnlyList<WalletTag> tags = walletService.GetAllTags();
        if (tags.Count == 0)
        {
            ConsoleHelper.ShowInformation("No tags have been created.");
            return;
        }

        Table table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Tag");
        foreach (WalletTag tag in tags)
        {
            table.AddRow(tag.Id.ToString(), Markup.Escape(tag.Name));
        }
        AnsiConsole.Write(table);
    }

    private WalletTag? TrySelectTag()
    {
        IReadOnlyList<WalletTag> tags = walletService.GetAllTags();
        if (tags.Count == 0)
        {
            ConsoleHelper.ShowInformation("No tags have been created.");
            return null;
        }

        return inputReader.ReadSelection(
            "Select a tag:",
            tags,
            tag => tag.Name
        );
    }

    private void OpenTransaction()
    {
        IReadOnlyList<WalletTransaction> transactions = walletService.GetAll();
        if (transactions.Count == 0)
        {
            ConsoleHelper.ShowInformation("No transactions have been recorded.");
            inputReader.WaitForContinue();
            return;
        }

        WalletTransaction transaction = SelectTransaction(transactions);
        ConsoleHelper.ShowHeader("Transaction");
        AnsiConsole.Write(
            new WalletTransactionCard(
                transaction,
                walletService.GetTagName(transaction.TagId)
            ).Build()
        );
        AnsiConsole.WriteLine();

        if (!transaction.IsReversed && !transaction.IsReversal &&
            inputReader.ReadConfirmation("Reverse this transaction?"))
        {
            ReverseTransaction(transaction);
        }

        inputReader.WaitForContinue();
    }

    private void ReverseTransaction(WalletTransaction transaction)
    {
        inputReader.ShowCancellationHint();
        try
        {
            string reason = inputReader.ReadRequiredStringOrCancel("Reversal reason:");
            walletService.ReverseTransaction(transaction, reason);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Transaction reversed successfully.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Reversal canceled.");
        }
    }

    private void ShowHistory()
    {
        ConsoleHelper.ShowHeader("Wallet History");
        IReadOnlyList<WalletTransaction> transactions = walletService.GetAll();
        if (transactions.Count == 0)
        {
            ConsoleHelper.ShowInformation("No transactions have been recorded.");
            return;
        }

        AnsiConsole.Write(
            new WalletTransactionTable(
                transactions,
                walletService.GetTagName
            ).Build()
        );
    }

    private void ShowMonthlySummary()
    {
        ConsoleHelper.ShowHeader("Monthly Summary");
        inputReader.ShowCancellationHint();

        try
        {
            int month = inputReader.ReadPositiveIntegerOrCancel("Month:");
            int year = inputReader.ReadPositiveIntegerOrCancel("Ano:");

            if (month > 12)
            {
                ConsoleHelper.ShowError("The month must be between 1 and 12.");
                return;
            }

            decimal balance = walletService.GetMonthlyBalance(year, month);
            AnsiConsole.MarkupLine(
                $"[bold]Resultado de {month:00}/{year}:[/] R$ {balance:N2}"
            );
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Query canceled.");
        }
    }

    private void ShowBalance()
    {
        string style = walletService.Balance >= 0 ? "green" : "red";
        AnsiConsole.MarkupLine(
            $"[bold]Current balance:[/] [{style}]R$ {walletService.Balance:N2}[/]"
        );
        AnsiConsole.WriteLine();
    }

    private WalletTransaction SelectTransaction(
        IEnumerable<WalletTransaction> transactions
    )
    {
        return inputReader.ReadSelection(
            "Select a transaction:",
            transactions,
            transaction =>
                $"{transaction.OccurredAt:dd/MM/yyyy} — " +
                $"{transaction.Description} — " +
                $"{walletService.GetTagName(transaction.TagId)} — " +
                $"R$ {transaction.Amount:N2}"
        );
    }
}
