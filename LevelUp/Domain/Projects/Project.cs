using System.Text.Json.Serialization;

namespace LevelUp.Domain.Projects;

public sealed class Project
{
    public int Id { get; set; }

    [JsonInclude]
    public string Name { get; private set; } =
        string.Empty;

    [JsonInclude]
    public string Description { get; private set; } =
        string.Empty;

    [JsonInclude]
    public string UnlockedTitle { get; private set; } =
        string.Empty;

    [JsonInclude]
    public ProjectStatus Status { get; private set; }
        = ProjectStatus.Created;

    public DateTime CreatedAt { get; init; }
        = DateTime.Now;

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? ArchivedAt { get; private set; }


    public void Configure(
        string name,
        string description,
        string unlockedTitle
    )
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException(
                "The project has already been configured."
            );
        }

        UpdateDetails(
            name,
            description,
            unlockedTitle
        );
    }

    public void UpdateDetails(
        string name,
        string description,
        string unlockedTitle
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description.Trim();
        UnlockedTitle = unlockedTitle.Trim();
    }

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