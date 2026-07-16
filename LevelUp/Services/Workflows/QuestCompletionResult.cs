using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Workflows;

public sealed record QuestCompletionResult(
    Quest Quest,
    Project? Project,
    Milestone? Milestone,
    Milestone? ActivatedMilestone,
    BossEncounter? UnlockedBoss,
    bool MilestoneCompleted,
    bool ProjectCompleted,
    decimal MilestoneProgress,
    decimal ProjectProgress
);
