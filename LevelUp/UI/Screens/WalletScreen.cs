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
            ConsoleHelper.ShowHeader("Carteira");
            ShowBalance();

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Registrar movimentação",
                    "Abrir movimentação",
                    "Ver histórico",
                    "Resumo mensal",
                    "Gerenciar tags",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Registrar movimentação":
                    CreateTransaction();
                    inputReader.WaitForContinue();
                    break;
                case "Abrir movimentação":
                    OpenTransaction();
                    break;
                case "Ver histórico":
                    ShowHistory();
                    inputReader.WaitForContinue();
                    break;
                case "Resumo mensal":
                    ShowMonthlySummary();
                    inputReader.WaitForContinue();
                    break;
                case "Gerenciar tags":
                    ManageTags();
                    break;
                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private void CreateTransaction()
    {
        ConsoleHelper.ShowHeader("Nova movimentação");
        inputReader.ShowCancellationHint();

        try
        {
            decimal amount = inputReader.ReadDecimalOrCancel(
                "Valor (positivo para crédito, negativo para débito):"
            );

            if (amount == 0)
            {
                ConsoleHelper.ShowError("O valor não pode ser zero.");
                return;
            }

            string description = inputReader.ReadRequiredStringOrCancel("Descrição:");
            WalletTag tag = SelectTagForTransaction();

            if (inputReader.ReadDecision("Confirmar movimentação?") != PromptDecision.Yes)
            {
                ConsoleHelper.ShowInformation("Movimentação cancelada.");
                return;
            }

            walletService.AddTransaction(amount, description, tag);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Movimentação registrada com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Movimentação cancelada.");
        }
    }

    private WalletTag SelectTagForTransaction()
    {
        IReadOnlyList<WalletTag> tags = walletService.GetAllTags();
        if (tags.Count == 0)
        {
            ConsoleHelper.ShowInformation(
                "Nenhuma tag foi cadastrada. Crie uma tag para continuar."
            );
            string name = inputReader.ReadRequiredStringOrCancel("Nome da nova tag:");
            WalletTag created = walletService.CreateTag(name);
            gameStateService.Save();
            return created;
        }

        return inputReader.ReadSelection(
            "Selecione uma tag:",
            tags,
            tag => tag.Name
        );
    }

    private void ManageTags()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Tags da Carteira");
            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Criar tag",
                    "Editar tag",
                    "Excluir tag",
                    "Listar tags",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Criar tag":
                    CreateTag();
                    inputReader.WaitForContinue();
                    break;
                case "Editar tag":
                    EditTag();
                    inputReader.WaitForContinue();
                    break;
                case "Excluir tag":
                    DeleteTag();
                    inputReader.WaitForContinue();
                    break;
                case "Listar tags":
                    ListTags();
                    inputReader.WaitForContinue();
                    break;
                case "Voltar":
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
            string name = inputReader.ReadRequiredStringOrCancel("Nome da tag:");
            walletService.CreateTag(name);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Tag criada com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Criação da tag cancelada.");
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
            string name = inputReader.ReadRequiredStringOrCancel("Novo nome:");
            walletService.UpdateTag(tag, name);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Tag atualizada com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Edição da tag cancelada.");
        }
    }

    private void DeleteTag()
    {
        WalletTag? tag = TrySelectTag();
        if (tag is null)
        {
            return;
        }

        if (!inputReader.ReadConfirmation($"Excluir a tag '{tag.Name}'?"))
        {
            ConsoleHelper.ShowInformation("Exclusão cancelada.");
            return;
        }

        walletService.DeleteTag(tag.Id);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Tag excluída com sucesso.");
    }

    private void ListTags()
    {
        IReadOnlyList<WalletTag> tags = walletService.GetAllTags();
        if (tags.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhuma tag foi cadastrada.");
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
            ConsoleHelper.ShowInformation("Nenhuma tag foi cadastrada.");
            return null;
        }

        return inputReader.ReadSelection(
            "Selecione uma tag:",
            tags,
            tag => tag.Name
        );
    }

    private void OpenTransaction()
    {
        IReadOnlyList<WalletTransaction> transactions = walletService.GetAll();
        if (transactions.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhuma movimentação foi registrada.");
            inputReader.WaitForContinue();
            return;
        }

        WalletTransaction transaction = SelectTransaction(transactions);
        ConsoleHelper.ShowHeader("Movimentação");
        AnsiConsole.Write(
            new WalletTransactionCard(
                transaction,
                walletService.GetTagName(transaction.TagId)
            ).Build()
        );
        AnsiConsole.WriteLine();

        if (!transaction.IsReversed && !transaction.IsReversal &&
            inputReader.ReadConfirmation("Estornar esta movimentação?"))
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
            string reason = inputReader.ReadRequiredStringOrCancel("Motivo do estorno:");
            walletService.ReverseTransaction(transaction, reason);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Movimentação estornada com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Estorno cancelado.");
        }
    }

    private void ShowHistory()
    {
        ConsoleHelper.ShowHeader("Histórico da carteira");
        IReadOnlyList<WalletTransaction> transactions = walletService.GetAll();
        if (transactions.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhuma movimentação foi registrada.");
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
        ConsoleHelper.ShowHeader("Resumo mensal");
        inputReader.ShowCancellationHint();

        try
        {
            int month = inputReader.ReadPositiveIntegerOrCancel("Mês:");
            int year = inputReader.ReadPositiveIntegerOrCancel("Ano:");

            if (month > 12)
            {
                ConsoleHelper.ShowError("O mês deve estar entre 1 e 12.");
                return;
            }

            decimal balance = walletService.GetMonthlyBalance(year, month);
            AnsiConsole.MarkupLine(
                $"[bold]Resultado de {month:00}/{year}:[/] R$ {balance:N2}"
            );
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Consulta cancelada.");
        }
    }

    private void ShowBalance()
    {
        string style = walletService.Balance >= 0 ? "green" : "red";
        AnsiConsole.MarkupLine(
            $"[bold]Saldo atual:[/] [{style}]R$ {walletService.Balance:N2}[/]"
        );
        AnsiConsole.WriteLine();
    }

    private WalletTransaction SelectTransaction(
        IEnumerable<WalletTransaction> transactions
    )
    {
        return inputReader.ReadSelection(
            "Selecione uma movimentação:",
            transactions,
            transaction =>
                $"{transaction.OccurredAt:dd/MM/yyyy} — " +
                $"{transaction.Description} — " +
                $"{walletService.GetTagName(transaction.TagId)} — " +
                $"R$ {transaction.Amount:N2}"
        );
    }
}
