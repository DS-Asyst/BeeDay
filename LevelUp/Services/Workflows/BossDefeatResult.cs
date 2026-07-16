using LevelUp.Domain.Achievements;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Projects;

namespace LevelUp.Services.Workflows;

public sealed record BossDefeatResult(
    BossEncounter Boss,
    Project Project,
    Achievement Achievement,
    bool ProjectCompleted
);
