using System.Text.Json.Serialization;

namespace LevelUp.Domain.Milestones;

public sealed class Milestone
{
    public int Id { get; set; }

    public int ProjectId { get; private set; }

    [JsonInclude]
    public string Title { get; private set; } =
        string.Empty;

    [JsonInclude]
    public string Description { get; private set; } =
        string.Empty;

    [JsonInclude]
    public MilestoneStatus Status { get; private set; }
        = MilestoneStatus.Created;

    public DateTime CreatedAt { get; init; }
        = DateTime.Now;

    [JsonInclude]
    public DateTime? ActivatedAt { get; private set; }

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? ArchivedAt { get; private set; }

    public void Configure(
        int projectId,
        string title,
        string description
    )
    {
        if (ProjectId > 0)
        {
            throw new InvalidOperationException(
                "The milestone has already been configured."
            );
        }

        if (projectId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectId),
                "A milestone must be associated with a valid project."
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        ProjectId = projectId;
        Title = title.Trim();
        Description = description.Trim();
    }

    public void UpdateDetails(
        string title,
        string description
    )
    {
        EnsureNotArchived();

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        Description = description.Trim();
    }

    public void Activate()
    {
        if (Status != MilestoneStatus.Created)
        {
            throw new InvalidOperationException(
                "Only created milestones can be activated."
            );
        }

        Status = MilestoneStatus.Active;
        ActivatedAt = DateTime.Now;
    }

    public void Complete()
    {
        if (Status != MilestoneStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active milestones can be completed."
            );
        }

        Status = MilestoneStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == MilestoneStatus.Archived)
        {
            throw new InvalidOperationException(
                "The milestone is already archived."
            );
        }

        Status = MilestoneStatus.Archived;
        ArchivedAt = DateTime.Now;
    }

    private void EnsureNotArchived()
    {
        if (Status == MilestoneStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived milestones cannot be modified."
            );
        }
    }
}