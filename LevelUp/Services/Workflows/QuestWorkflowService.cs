using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class QuestWorkflowService
{
    private readonly QuestService questService;
    private readonly ProjectService projectService;
    private readonly GameStateService gameStateService;

    public QuestWorkflowService(
        QuestService questService,
        ProjectService projectService,
        GameStateService gameStateService
    )
    {
        this.questService = questService;
        this.projectService = projectService;
        this.gameStateService = gameStateService;
    }

    public QuestCompletionResult CompleteQuest(int questId)
    {
        Quest quest = questService.GetQuestById(questId)
            ?? throw new InvalidOperationException(
                "The selected quest was not found."
            );

        questService.CompleteQuest(quest);

        Project? project = quest.ProjectId is null
            ? null
            : projectService.GetProjectById(quest.ProjectId.Value);

        bool projectCompleted = project is not null &&
            projectService.TryCompleteProject(
                project,
                questService.GetAllQuests()
            );

        decimal progress = project is null
            ? 0m
            : projectService.CalculateProgress(
                project,
                questService.GetAllQuests()
            );

        gameStateService.Save();

        return new QuestCompletionResult(
            quest,
            project,
            projectCompleted,
            progress
        );
    }
}
