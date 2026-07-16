using LevelUp.Domain;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class MigrationV2ToV3 : IGameDataMigration
{
    public int SourceVersion => 2;
    public int TargetVersion => 3;

    public void Apply(GameData gameData)
    {
        gameData.Goals ??= [];
        gameData.SchemaVersion = TargetVersion;
    }
}
