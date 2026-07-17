using LevelUp.Domain.Wallet;
using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class WalletServiceTests
{
    [Fact]
    public void EntryAndExit_ShouldCalculateBalance()
    {
        WalletService service = new();

        service.AddDeposit(
            1000m,
            "Reserva mensal",
            new DateTime(2026, 7, 1)
        );
        service.AddWithdrawal(
            250m,
            "Compra necessária",
            "Substituição de equipamento",
            new DateTime(2026, 7, 10)
        );

        Assert.Equal(750m, service.Balance);
    }

    [Fact]
    public void Exit_ShouldAllowNegativeBalance()
    {
        WalletService service = new();

        service.AddDeposit(
            100m,
            "Entrada inicial",
            new DateTime(2026, 7, 1)
        );
        service.AddWithdrawal(
            150m,
            "Pagamento ao irmão",
            "Valor emprestado",
            new DateTime(2026, 7, 2)
        );

        Assert.Equal(-50m, service.Balance);
    }

    [Fact]
    public void Exit_ShouldUseSelectedTag()
    {
        WalletService service = new();

        WalletTag tag =
            service.CreateTag("Empréstimo");

        WalletTransaction transaction =
            service.AddExit(
                50m,
                "Pagamento ao irmão",
                tag,
                new DateTime(2026, 7, 2)
            );

        Assert.Equal(tag.Id, transaction.TagId);
    }

    [Fact]
    public void DeletingEntry_ShouldAllowNegativeResultingBalance()
    {
        WalletService service = new();
        WalletTransaction entry = service.AddDeposit(
            100m,
            "Entrada",
            new DateTime(2026, 7, 1)
        );
        service.AddWithdrawal(
            50m,
            "Saída",
            "Teste",
            new DateTime(2026, 7, 2)
        );

        bool deleted = service.DeleteTransaction(entry.Id);

        Assert.True(deleted);
        Assert.Equal(-50m, service.Balance);
    }
}
