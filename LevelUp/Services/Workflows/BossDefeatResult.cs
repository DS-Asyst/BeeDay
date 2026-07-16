using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;

namespace LevelUp.Services.Workflows;

public sealed record BossDefeatResult(
    BossEncounter Boss,
    Milestone Milestone,
    Milestone? ActivatedMilestone,
    Project Project,
    bool ProjectCompleted
);
