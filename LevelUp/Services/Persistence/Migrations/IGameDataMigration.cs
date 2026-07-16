using LevelUp.Domain;

namespace LevelUp.Services.Persistence.Migrations;

public interface IGameDataMigration
{
    int SourceVersion { get; }
    int TargetVersion { get; }
    void Apply(GameData gameData);
}
