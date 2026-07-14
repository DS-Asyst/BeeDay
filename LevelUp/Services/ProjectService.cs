using LevelUp.Domain.Projects;

namespace LevelUp.Services;


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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Project project = new()
        {
            Id = nextId++,
            Name = name.Trim(),
            Description = description.Trim(),
            UnlockedTitle = unlockedTitle.Trim()
        };

        projects.Add(project);

        return project;
    }

    public IReadOnlyList<Project> GetAllProjects()
    {
        return projects.AsReadOnly();
    }

    public Project? GetProjectById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return projects.FirstOrDefault(
            project => project.Id == id
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

    public void ArchiveProject(Project project)
    {
        EnsureManagedProject(project);

        project.Archive();
    }

    public bool DeleteProject(int id)
    {
        Project? project = GetProjectById(id);

        if (project is null)
        {
            return false;
        }

        return projects.Remove(project);
    }

    public decimal CalculateProgress(Project project)
    {
        EnsureManagedProject(project);

        // Será calculado por meio das quests vinculadas
        // quando o domínio de Quest for implementado.
        return 0m;
    }

    public bool IsCompleted(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return project.Status ==
            ProjectStatus.Completed;
    }

    private void EnsureManagedProject(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        bool projectExists = projects.Any(
            existingProject =>
                existingProject.Id == project.Id
        );

        if (!projectExists)
        {
            throw new InvalidOperationException(
                "The project is not managed by this service."
            );
        }
    }
}