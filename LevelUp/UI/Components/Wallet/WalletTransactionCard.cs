using LevelUp.Domain.Wallet;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Wallet;

public sealed class WalletTransactionCard
{
    private readonly WalletTransaction transaction;

    public WalletTransactionCard(WalletTransaction transaction)
    {
        this.transaction = transaction;
    }

    public Panel Build()
    {
        string type = transaction.Type == WalletTransactionType.Deposit
            ? "Depósito"
            : "Retirada";

        EntityCard card = new EntityCard(
            transaction.Description,
            UIIcons.Gold
        )
            .AddText("Tipo", type)
            .AddText("Valor", $"R$ {transaction.Amount:N2}")
            .AddText("Data", transaction.OccurredAt.ToString("dd/MM/yyyy"));

        if (!string.IsNullOrWhiteSpace(transaction.Justification))
        {
            card.AddText("Justificativa", transaction.Justification);
        }

        return card.Build();
    }
}
