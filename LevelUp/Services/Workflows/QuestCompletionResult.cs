using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Workflows;

public sealed record QuestCompletionResult(
    Quest Quest,
    Project? Project,
    bool ProjectCompleted,
    decimal ProjectProgress
);
