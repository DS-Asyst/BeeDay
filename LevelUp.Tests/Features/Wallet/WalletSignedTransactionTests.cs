using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class WalletSignedTransactionTests
{
    [Fact]
    public void AddTransaction_ShouldUseSignedAmountToCalculateBalance()
    {
        WalletService service = new();
        var tag = service.CreateTag("Teste");

        service.AddTransaction(100m, "Crédito", tag, new DateTime(2026, 7, 1));
        service.AddTransaction(-140m, "Débito", tag, new DateTime(2026, 7, 2));

        Assert.Equal(-40m, service.Balance);
    }

    [Fact]
    public void AddTransaction_ShouldRejectZero()
    {
        WalletService service = new();
        var tag = service.CreateTag("Teste");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.AddTransaction(0m, "Inválida", tag)
        );
    }
}
