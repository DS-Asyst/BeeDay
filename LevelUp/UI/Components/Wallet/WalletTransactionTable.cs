using LevelUp.Domain.Wallet;
using Spectre.Console;

namespace LevelUp.UI.Components.Wallet;

public sealed class WalletTransactionTable
{
    private readonly IEnumerable<WalletTransaction> transactions;

    public WalletTransactionTable(
        IEnumerable<WalletTransaction> transactions
    )
    {
        this.transactions = transactions;
    }

    public Table Build()
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .Expand();

        table.AddColumn("Data");
        table.AddColumn("Tipo");
        table.AddColumn("Descrição");
        table.AddColumn(new TableColumn("Valor").RightAligned());
        table.AddColumn("Justificativa");

        foreach (WalletTransaction transaction in transactions)
        {
            string type = transaction.Type == WalletTransactionType.Deposit
                ? "Depósito"
                : "Retirada";
            string amount = transaction.Type == WalletTransactionType.Deposit
                ? $"[green]+ R$ {transaction.Amount:N2}[/]"
                : $"[red]- R$ {transaction.Amount:N2}[/]";

            table.AddRow(
                transaction.OccurredAt.ToString("dd/MM/yyyy"),
                type,
                Markup.Escape(transaction.Description),
                amount,
                Markup.Escape(
                    string.IsNullOrWhiteSpace(transaction.Justification)
                        ? "—"
                        : transaction.Justification
                )
            );
        }

        return table;
    }
}
