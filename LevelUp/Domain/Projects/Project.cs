namespace LevelUp.Domain.Projects;

public sealed class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; private set; }
        = ProjectStatus.Created;

    public string UnlockedTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; init; }
        = DateTime.Now;

    public DateTime? CompletedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public void Activate()
    {
        if (Status != ProjectStatus.Created)
        {
            throw new InvalidOperationException(
                "Only created projects can be activated."
            );
        }

        Status = ProjectStatus.Active;
    }

    public void Complete()
    {
        if (Status != ProjectStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active projects can be completed."
            );
        }

        Status = ProjectStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == ProjectStatus.Archived)
        {
            throw new InvalidOperationException(
                "The project is already archived."
            );
        }

        Status = ProjectStatus.Archived;
        ArchivedAt = DateTime.Now;
    }
}