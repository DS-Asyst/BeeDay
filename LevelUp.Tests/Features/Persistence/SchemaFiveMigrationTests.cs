using LevelUp.Domain;
using LevelUp.Domain.Wallet;
using LevelUp.Services.Persistence.Migrations;
using Xunit;

namespace LevelUp.Tests;

public sealed class SchemaFiveMigrationTests
{
    [Fact]
    public void Migration_ShouldAssignDefaultTagToLegacyTransactions()
    {
        WalletTransaction transaction = new() { Id = 1 };
        transaction.Configure(
            WalletTransactionType.Withdrawal,
            100m,
            "Legacy exit",
            "Legacy justification",
            new DateTime(2026, 7, 1)
        );
        GameData data = new()
        {
            SchemaVersion = 4,
            WalletTransactions = [transaction]
        };

        new GameDataMigrator().Migrate(data);

        Assert.Equal(GameData.CurrentSchemaVersion, data.SchemaVersion);
        WalletTag tag = Assert.Single(data.WalletTags);
        Assert.Equal("Sem tag", tag.Name);
        Assert.Equal(tag.Id, transaction.TagId);
    }
}
