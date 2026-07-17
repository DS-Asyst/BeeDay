using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class PersistenceReliabilityTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"LevelUpPersistence_{Guid.NewGuid():N}");

    [Fact]
    public void ReverseTransaction_ShouldPreserveHistoryAndRestoreBalance()
    {
        WalletService service = new();
        var deposit = service.AddDeposit(500m, "Reserva", new DateTime(2026, 7, 1));

        var reversal = service.ReverseTransaction(
            deposit,
            "Lançamento duplicado",
            new DateTime(2026, 7, 2)
        );

        Assert.Equal(0m, service.Balance);
        Assert.True(deposit.IsReversed);
        Assert.True(reversal.IsReversal);
        Assert.Equal(deposit.Id, reversal.ReversalOfTransactionId);
        Assert.Equal(2, service.GetAll().Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
