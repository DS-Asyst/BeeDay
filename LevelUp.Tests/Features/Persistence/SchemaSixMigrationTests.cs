using LevelUp.Domain;
using LevelUp.Domain.Wallet;
using LevelUp.Services.Persistence.Migrations;
using Xunit;

namespace LevelUp.Tests;

public sealed class SchemaSixMigrationTests
{
    [Fact]
    public void Migration_ShouldConvertLegacyWithdrawalToNegativeAmount()
    {
        WalletTransaction transaction = new() { Id = 1 };
        transaction.Configure(
            WalletTransactionType.Withdrawal,
            75m,
            "Saída antiga",
            string.Empty,
            new DateTime(2026, 7, 1)
        );

        GameData data = new()
        {
            SchemaVersion = 5,
            WalletTransactions = [transaction]
        };

        new GameDataMigrator().Migrate(data);

        Assert.Equal(GameData.CurrentSchemaVersion, data.SchemaVersion);
        Assert.Equal(-75m, transaction.Amount);
    }
}
