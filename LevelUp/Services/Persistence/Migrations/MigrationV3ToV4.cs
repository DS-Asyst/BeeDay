using LevelUp.Domain;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class MigrationV3ToV4 : IGameDataMigration
{
    public int SourceVersion => 3;
    public int TargetVersion => 4;

    public void Apply(GameData gameData)
    {
        // O schema 4 remove o antigo módulo de metas.
        // Dados desconhecidos do JSON legado são ignorados pelo desserializador.
        gameData.SchemaVersion = TargetVersion;
    }
}
