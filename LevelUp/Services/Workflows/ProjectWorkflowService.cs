using LevelUp.Domain.Projects;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Services.Workflows;

public sealed class ProjectWorkflowService
{
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly GameStateService gameStateService;

    public ProjectWorkflowService(
        ProjectService projectService,
        QuestService questService,
        GameStateService gameStateService
    )
    {
        this.projectService = projectService;
        this.questService = questService;
        this.gameStateService = gameStateService;
    }

    public bool DeleteProject(int projectId)
    {
        Project? project = projectService.GetProjectById(projectId);
        if (project is null)
        {
            return false;
        }

        foreach (var quest in questService.GetQuestsByProjectId(projectId))
        {
            questService.RemoveQuestFromProject(quest);
        }

        bool deleted = projectService.DeleteProject(projectId);
        if (deleted)
        {
            gameStateService.Save();
        }

        return deleted;
    }
}
