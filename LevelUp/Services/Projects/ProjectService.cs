using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Attributes;
using LevelUp.Domain.Quests;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.Services.Projects;

public sealed class ProjectService
{
    private readonly List<Project> projects = [];
    private int nextId = 1;

    public ProjectService(IEnumerable<Project>? projects = null)
    {
        if (projects is null)
        {
            return;
        }

        this.projects.AddRange(projects);
        if (this.projects.Count > 0)
        {
            nextId = this.projects.Max(project => project.Id) + 1;
        }
    }

    public Project CreateProject(string name, string description)
        => CreateProject(name, description, AttributeType.Intelligence);

    public Project CreateProject(
        string name,
        string description,
        AttributeType primaryAttribute
    )
    {
        Project project = new() { Id = nextId++ };
        project.Configure(name, description, primaryAttribute);
        projects.Add(project);
        return project;
    }

    public Project CreateProject(string name, string description, string legacyUnlockedTitle)
    {
        return CreateProject(name, description);
    }

    public IReadOnlyList<Project> GetAllProjects() => projects.AsReadOnly();

    public Project? GetProjectById(int id)
    {
        return id <= 0 ? null : projects.FirstOrDefault(project => project.Id == id);
    }

    public void UpdateProject(Project project, string name, string description)
    {
        EnsureManagedProject(project);
        project.UpdateDetails(name, description);
    }

    public void UpdateProject(Project project, string name, string description, string legacyUnlockedTitle)
    {
        UpdateProject(project, name, description);
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
        IEnumerable<QuestModel> quests
    )
    {
        EnsureManagedProject(project);
        if (project.Status != ProjectStatus.Active || !HasCompletedAllQuests(project, quests))
        {
            return false;
        }
        project.Complete();
        return true;
    }

    public bool TryCompleteProject(
        Project project,
        IEnumerable<QuestModel> quests,
        IEnumerable<Milestone> milestones
    )
    {
        if (!AreCompletionRequirementsMet(project, quests, milestones))
        {
            return false;
        }
        project.Complete();
        return true;
    }

    public bool AreCompletionRequirementsMet(
        Project project,
        IEnumerable<QuestModel> quests,
        IEnumerable<Milestone> milestones
    )
    {
        EnsureManagedProject(project);
        ArgumentNullException.ThrowIfNull(milestones);

        List<Milestone> validMilestones = milestones
            .Where(item => item.ProjectId == project.Id && item.Status != MilestoneStatus.Archived)
            .ToList();
        List<Quest> validQuests = GetProgressQuests(project, quests);
        bool hasProgressItems = validMilestones.Count > 0 || validQuests.Count > 0;
        bool milestonesCompleted = validMilestones.All(
            item => item.Status == MilestoneStatus.Completed
        );
        bool questsCompleted = validQuests.All(
            item => item.Status == QuestStatus.Completed
        );

        return hasProgressItems && milestonesCompleted && questsCompleted;
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

    public decimal CalculateProgress(Project project, IEnumerable<QuestModel> quests)
    {
        List<Quest> projectQuests = GetProgressQuests(project, quests);
        if (projectQuests.Count == 0)
        {
            return 0m;
        }

        int completed = projectQuests.Count(quest => quest.Status == QuestStatus.Completed);
        return Math.Round(completed * 100m / projectQuests.Count, 2);
    }

    public bool HasCompletedAllQuests(Project project, IEnumerable<QuestModel> quests)
    {
        List<Quest> projectQuests = GetProgressQuests(project, quests);
        return projectQuests.Count > 0 && projectQuests.All(quest => quest.Status == QuestStatus.Completed);
    }

    public bool IsCompleted(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return project.Status == ProjectStatus.Completed;
    }

    private List<Quest> GetProgressQuests(Project project, IEnumerable<QuestModel> quests)
    {
        EnsureManagedProject(project);
        ArgumentNullException.ThrowIfNull(quests);
        return quests
            .Where(quest => quest.ProjectId == project.Id && quest.Status != QuestStatus.Archived)
            .ToList();
    }

    private void EnsureManagedProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (!projects.Any(existing => existing.Id == project.Id))
        {
            throw new InvalidOperationException("O projeto não é gerenciado por este serviço.");
        }
    }
}
