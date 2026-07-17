using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Rewards;

namespace LevelUp.Services.Workflows;

public sealed record QuestCompletionResult(
    Quest Quest,
    Project? Project,
    Milestone? Milestone,
    Milestone? ActivatedMilestone,
    Quest? ActivatedQuest,
    BossEncounter? UnlockedBoss,
    bool MilestoneCompleted,
    bool ProjectCompleted,
    decimal MilestoneProgress,
    decimal ProjectProgress,
    Reward Reward
);
