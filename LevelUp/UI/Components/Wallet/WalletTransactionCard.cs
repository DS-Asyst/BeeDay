using LevelUp.Domain.Wallet;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Wallet;

public sealed class WalletTransactionCard
{
    private readonly WalletTransaction transaction;
    private readonly string tagName;

    public WalletTransactionCard(
        WalletTransaction transaction,
        string tagName
    )
    {
        this.transaction = transaction;
        this.tagName = tagName;
    }

    public Panel Build()
    {
        string signedValue = transaction.Amount > 0
            ? $"+ R$ {transaction.Amount:N2}"
            : $"- R$ {Math.Abs(transaction.Amount):N2}";

        EntityCard card = new EntityCard(
            transaction.Description,
            UIIcons.Gold
        )
            .AddText("Tag", tagName)
            .AddText("Amount", signedValue)
            .AddText("Date", transaction.OccurredAt.ToString("dd/MM/yyyy"));

        if (transaction.IsReversal)
        {
            card.AddText(
                "Transaction original",
                $"#{transaction.ReversalOfTransactionId}"
            );
            card.AddText("Reversal Reason", transaction.ReversalReason);
        }

        if (transaction.IsReversed)
        {
            card.AddText(
                "Estornada em",
                transaction.ReversedAt?.ToString("dd/MM/yyyy") ?? "—"
            );
        }

        return card.Build();
    }
}
