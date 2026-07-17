using LevelUp.Domain.Wallet;
using Spectre.Console;

namespace LevelUp.UI.Components.Wallet;

public sealed class WalletTransactionTable
{
    private readonly IEnumerable<WalletTransaction> transactions;
    private readonly Func<int?, string> tagNameResolver;

    public WalletTransactionTable(
        IEnumerable<WalletTransaction> transactions,
        Func<int?, string> tagNameResolver
    )
    {
        this.transactions = transactions;
        this.tagNameResolver = tagNameResolver;
    }

    public Table Build()
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .Expand();

        table.AddColumn("Data");
        table.AddColumn("Descrição");
        table.AddColumn("Tag");
        table.AddColumn(new TableColumn("Valor").RightAligned());
        table.AddColumn("Situação");

        foreach (WalletTransaction transaction in transactions)
        {
            string amount = transaction.Amount > 0
                ? $"[green]+ R$ {transaction.Amount:N2}[/]"
                : $"[red]- R$ {Math.Abs(transaction.Amount):N2}[/]";

            table.AddRow(
                transaction.OccurredAt.ToString("dd/MM/yyyy"),
                Markup.Escape(transaction.Description),
                Markup.Escape(tagNameResolver(transaction.TagId)),
                amount,
                transaction.IsReversal
                    ? "Estorno"
                    : transaction.IsReversed
                        ? "Estornada"
                        : "Confirmada"
            );
        }

        return table;
    }
}
