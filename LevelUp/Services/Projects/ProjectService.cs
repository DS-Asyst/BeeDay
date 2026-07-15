using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Projects;

public sealed class ProjectService
{
    private readonly List<Project> projects = [];
    private int nextId = 1;

    public ProjectService(
        IEnumerable<Project>? projects = null
    )
    {
        if (projects is null)
        {
            return;
        }

        this.projects.AddRange(projects);

        if (this.projects.Count > 0)
        {
            nextId = this.projects.Max(
                project => project.Id
            ) + 1;
        }
    }

    public Project CreateProject(
        string name,
        string description,
        string unlockedTitle
    )
    {
        Project project = new()
        {
            Id = nextId++
        };

        project.Configure(
            name,
            description,
            unlockedTitle
        );

        projects.Add(project);
        return project;
    }

    public IReadOnlyList<Project> GetAllProjects()
    {
        return projects.AsReadOnly();
    }

    public Project? GetProjectById(int id)
    {
        return id <= 0
            ? null
            : projects.FirstOrDefault(
                project => project.Id == id
            );
    }

    public void UpdateProject(
        Project project,
        string name,
        string description,
        string unlockedTitle
    )
    {
        EnsureManagedProject(project);
        project.UpdateDetails(
            name,
            description,
            unlockedTitle
        );
    }

    public void ActivateProject(Project project)
    {
        EnsureManagedProject(project);
        project.Activate();
    }

    public void CompleteProject(Project project)
    {
        EnsureManagedProject(project);
        project.Complete();
    }

    public bool TryCompleteProject(
        Project project,
        IEnumerable<Quest> quests
    )
    {
        EnsureManagedProject(project);

        if (project.Status != ProjectStatus.Active ||
            !HasCompletedAllQuests(project, quests))
        {
            return false;
        }

        project.Complete();
        return true;
    }

    public void ArchiveProject(Project project)
    {
        EnsureManagedProject(project);
        project.Archive();
    }

    public bool DeleteProject(int id)
    {
        Project? project = GetProjectById(id);
        return project is not null && projects.Remove(project);
    }

    public decimal CalculateProgress(
        Project project,
        IEnumerable<Quest> quests
    )
    {
        List<Quest> projectQuests = GetProgressQuests(
            project,
            quests
        );

        if (projectQuests.Count == 0)
        {
            return 0m;
        }

        int completedQuests = projectQuests.Count(
            quest => quest.Status == QuestStatus.Completed
        );

        return Math.Round(
            completedQuests * 100m / projectQuests.Count,
            2
        );
    }

    public bool HasCompletedAllQuests(
        Project project,
        IEnumerable<Quest> quests
    )
    {
        List<Quest> projectQuests = GetProgressQuests(
            project,
            quests
        );

        return projectQuests.Count > 0 &&
            projectQuests.All(
                quest => quest.Status == QuestStatus.Completed
            );
    }

    public bool IsCompleted(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return project.Status == ProjectStatus.Completed;
    }

    private List<Quest> GetProgressQuests(
        Project project,
        IEnumerable<Quest> quests
    )
    {
        EnsureManagedProject(project);
        ArgumentNullException.ThrowIfNull(quests);

        return quests
            .Where(quest =>
                quest.ProjectId == project.Id &&
                quest.Status != QuestStatus.Archived
            )
            .ToList();
    }

    private void EnsureManagedProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (!projects.Any(
            existingProject => existingProject.Id == project.Id
        ))
        {
            throw new InvalidOperationException(
                "The project is not managed by this service."
            );
        }
    }
}
