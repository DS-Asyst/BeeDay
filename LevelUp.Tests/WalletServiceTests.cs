using LevelUp.Domain.Wallet;
using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class WalletServiceTests
{
    [Fact]
    public void DepositAndWithdrawal_ShouldCalculateBalance()
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
    public void Withdrawal_ShouldRequireAvailableBalance()
    {
        WalletService service = new();
        service.AddDeposit(
            100m,
            "Reserva",
            new DateTime(2026, 7, 1)
        );

        Assert.Throws<InvalidOperationException>(
            () => service.AddWithdrawal(
                101m,
                "Retirada",
                "Teste",
                new DateTime(2026, 7, 2)
            )
        );
    }

    [Fact]
    public void Withdrawal_ShouldRequireJustification()
    {
        WalletService service = new();
        service.AddDeposit(
            100m,
            "Reserva",
            new DateTime(2026, 7, 1)
        );

        Assert.Throws<ArgumentException>(
            () => service.AddWithdrawal(
                50m,
                "Retirada",
                string.Empty,
                new DateTime(2026, 7, 2)
            )
        );
    }

    [Fact]
    public void DeleteDeposit_ShouldRejectNegativeResultingBalance()
    {
        WalletService service = new();
        WalletTransaction deposit = service.AddDeposit(
            100m,
            "Reserva",
            new DateTime(2026, 7, 1)
        );
        service.AddWithdrawal(
            50m,
            "Retirada",
            "Teste",
            new DateTime(2026, 7, 2)
        );

        Assert.Throws<InvalidOperationException>(
            () => service.DeleteTransaction(deposit.Id)
        );
    }
}
