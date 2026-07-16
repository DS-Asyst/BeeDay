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
                    "Registrar entrada",
                    "Registrar saída",
                    "Abrir movimentação",
                    "Ver histórico",
                    "Resumo mensal",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Registrar entrada":
                    CreateDeposit();
                    inputReader.WaitForContinue();
                    break;

                case "Registrar saída":
                    CreateWithdrawal();
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

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private void CreateDeposit()
    {
        ConsoleHelper.ShowHeader("Nova entrada");
        inputReader.ShowCancellationHint();

        try
        {
            decimal amount = inputReader.ReadPositiveDecimalOrCancel(
                "Valor:"
            );
            string description = inputReader.ReadRequiredStringOrCancel(
                "Descrição:"
            );
            DateTime date = inputReader.ReadDateOrCancel(
                "Data da entrada (dd/MM/aaaa):"
            );

            PromptDecision decision = inputReader.ReadDecision(
                "Confirmar entrada?"
            );

            if (decision != PromptDecision.Yes)
            {
                ConsoleHelper.ShowInformation("Entrada cancelada.");
                return;
            }

            walletService.AddDeposit(amount, description, date);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Entrada registrada com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Entrada cancelada.");
        }
    }

    private void CreateWithdrawal()
    {
        ConsoleHelper.ShowHeader("Nova saída");
        inputReader.ShowCancellationHint();

        try
        {
            decimal amount = inputReader.ReadPositiveDecimalOrCancel(
                "Valor:"
            );
            string description = inputReader.ReadRequiredStringOrCancel(
                "Descrição:"
            );
            string justification = inputReader.ReadRequiredStringOrCancel(
                "Justificativa da saída:"
            );
            DateTime date = inputReader.ReadDateOrCancel(
                "Data da saída (dd/MM/aaaa):"
            );

            PromptDecision decision = inputReader.ReadDecision(
                "Confirmar saída?"
            );

            if (decision != PromptDecision.Yes)
            {
                ConsoleHelper.ShowInformation("Saída cancelada.");
                return;
            }

            walletService.AddWithdrawal(
                amount,
                description,
                justification,
                date
            );
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Saída registrada com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Saída cancelada.");
        }
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
        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Movimentação");
            AnsiConsole.Write(new WalletTransactionCard(transaction).Build());
            AnsiConsole.WriteLine();

            string action = inputReader.ReadSelection(
                "Escolha uma ação:",
                transaction.IsReversed || transaction.IsReversal
                    ? new[] { "Voltar" }
                    : new[] { "Estornar", "Voltar" },
                choice => choice
            );

            switch (action)
            {
                case "Estornar":
                    ReverseTransaction(transaction);
                    opened = false;
                    inputReader.WaitForContinue();
                    break;

                case "Voltar":
                    opened = false;
                    break;
            }
        }
    }

    private void ReverseTransaction(WalletTransaction transaction)
    {
        ConsoleHelper.ShowHeader("Estornar movimentação");
        inputReader.ShowCancellationHint();

        try
        {
            string reason = inputReader.ReadRequiredStringOrCancel(
                "Motivo do estorno:"
            );
            DateTime date = inputReader.ReadDateOrCancel(
                "Data do estorno (dd/MM/aaaa):"
            );

            if (!inputReader.ReadConfirmation("Confirmar estorno?"))
            {
                ConsoleHelper.ShowInformation("Estorno cancelado.");
                return;
            }

            walletService.ReverseTransaction(transaction, reason, date);
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

        AnsiConsole.Write(new WalletTransactionTable(transactions).Build());
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
                $"[bold]Resultado de {month:00}/{year}:[/] " +
                $"R$ {balance:N2}"
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
                $"{transaction.Description} — R$ {transaction.Amount:N2}"
        );
    }
}
