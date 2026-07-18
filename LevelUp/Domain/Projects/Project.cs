using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Projects;

public sealed class Project
{
    public int Id { get; set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public AttributeType PrimaryAttribute { get; private set; } = AttributeType.Intelligence;

    public ProjectStatus Status { get; private set; } = ProjectStatus.Created;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? CompletedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public void Configure(string name, string description)
        => Configure(name, description, AttributeType.Intelligence);

    public void Configure(string name, string description, AttributeType primaryAttribute)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("The project has already been configured.");
        }

        PrimaryAttribute = primaryAttribute;
        SetDetails(name, description);
    }

    public void Configure(
        string name,
        string description,
        string legacyUnlockedTitle
    )
    {
        Configure(name, description);
    }

    public void UpdateDetails(string name, string description)
    {
        EnsureNotArchived();
        SetDetails(name, description);
    }

    public void UpdateDetails(
        string name,
        string description,
        string legacyUnlockedTitle
    )
    {
        UpdateDetails(name, description);
    }

    public void Activate()
    {
        if (Status != ProjectStatus.Created)
        {
            throw new InvalidOperationException("Only created projects can be activated.");
        }

        Status = ProjectStatus.Active;
    }

    public void Complete()
    {
        if (Status != ProjectStatus.Active)
        {
            throw new InvalidOperationException("Only active projects can be completed.");
        }

        Status = ProjectStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == ProjectStatus.Archived)
        {
            throw new InvalidOperationException("The project is already archived.");
        }

        Status = ProjectStatus.Archived;
        ArchivedAt = DateTime.Now;
    }

    private void SetDetails(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description.Trim();
    }

    private void EnsureNotArchived()
    {
        if (Status == ProjectStatus.Archived)
        {
            throw new InvalidOperationException("Archived projects cannot be changed.");
        }
    }
}
