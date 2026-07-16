using LevelUp.Domain;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class GameDataMigrator
{
    private readonly IReadOnlyDictionary<int, IGameDataMigration> migrations;

    public GameDataMigrator(IEnumerable<IGameDataMigration>? migrations = null)
    {
        IEnumerable<IGameDataMigration> configured = migrations ??
            [new MigrationV1ToV2()];
        this.migrations = configured.ToDictionary(item => item.SourceVersion);
    }

    public void Migrate(GameData gameData)
    {
        if (gameData.SchemaVersion <= 0)
        {
            gameData.SchemaVersion = 1;
        }

        if (gameData.SchemaVersion > GameData.CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"O save usa o schema {gameData.SchemaVersion}, mais recente que o suportado."
            );
        }

        while (gameData.SchemaVersion < GameData.CurrentSchemaVersion)
        {
            if (!migrations.TryGetValue(gameData.SchemaVersion, out var migration))
            {
                throw new InvalidOperationException(
                    $"Não existe migração para o schema {gameData.SchemaVersion}."
                );
            }

            migration.Apply(gameData);
        }
    }
}
