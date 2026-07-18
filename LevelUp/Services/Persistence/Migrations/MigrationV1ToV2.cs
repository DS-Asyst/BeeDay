using LevelUp.Domain;
using LevelUp.Domain.Bosses;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class MigrationV1ToV2 : IGameDataMigration
{
    public int SourceVersion => 1;
    public int TargetVersion => 2;

    public void Apply(GameData gameData)
    {
        int nextBossId = gameData.Bosses.Count == 0
            ? 1
            : gameData.Bosses.Max(boss => boss.Id) + 1;

        foreach (var project in gameData.Projects)
        {
            bool hasFinalBoss = gameData.Bosses.Any(
                boss => boss.ProjectId == project.Id && boss.MilestoneId is null
            );

            if (hasFinalBoss)
            {
                continue;
            }

            BossEncounter boss = new() { Id = nextBossId++ };
            boss.Configure(
                project.Id,
                project.Name,
                "Final boss migrated from an earlier version.",
                "Specialist em"
            );
            gameData.Bosses.Add(boss);
        }

        gameData.SchemaVersion = TargetVersion;
    }
}
