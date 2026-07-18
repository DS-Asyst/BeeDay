using LevelUp.Domain;
using LevelUp.Domain.Wallet;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class MigrationV4ToV5 : IGameDataMigration
{
    public int SourceVersion => 4;
    public int TargetVersion => 5;

    public void Apply(GameData gameData)
    {
        gameData.WalletTags ??= [];

        if (gameData.WalletTransactions.Count > 0)
        {
            WalletTag defaultTag = gameData.WalletTags.FirstOrDefault()
                ?? CreateDefaultTag(gameData);

            foreach (WalletTransaction transaction in gameData.WalletTransactions)
            {
                if (transaction.TagId is null)
                {
                    transaction.AssignTagForMigration(defaultTag.Id);
                }
            }
        }

        gameData.SchemaVersion = TargetVersion;
    }

    private static WalletTag CreateDefaultTag(GameData gameData)
    {
        int nextId = gameData.WalletTags.Count == 0
            ? 1
            : gameData.WalletTags.Max(tag => tag.Id) + 1;

        WalletTag tag = new() { Id = nextId };
        tag.Configure("No tag");
        gameData.WalletTags.Add(tag);
        return tag;
    }
}
