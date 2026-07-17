using LevelUp.Domain;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class MigrationV5ToV6 : IGameDataMigration
{
    public int SourceVersion => 5;
    public int TargetVersion => 6;

    public void Apply(GameData gameData)
    {
        foreach (var transaction in gameData.WalletTransactions)
        {
            transaction.ConvertLegacyAmountToSigned();
        }

        gameData.SchemaVersion = TargetVersion;
    }
}
