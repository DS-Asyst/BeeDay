using System.Text.Json.Serialization;

namespace LevelUp.Domain.Quests;

public sealed class Quest
{
    public int Id { get; set; }

    [JsonInclude]
    public int? ProjectId { get; private set; }

    [JsonInclude]
    public int? MilestoneId { get; private set; }

    [JsonInclude]
    public string Title { get; private set; } = string.Empty;

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public QuestStatus Status { get; private set; } = QuestStatus.Created;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [JsonInclude]
    public DateTime? ActivatedAt { get; private set; }

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? ArchivedAt { get; private set; }

    public void Configure(string title, string description)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException(
                "The quest has already been configured."
            );
        }

        SetDetails(title, description);
    }

    public void UpdateDetails(string title, string description)
    {
        EnsureNotArchived();
        SetDetails(title, description);
    }

    public void AssignToProject(int projectId)
    {
        EnsureAssociationCanChange();

        if (projectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(projectId));
        }

        if (MilestoneId is not null && ProjectId != projectId)
        {
            throw new InvalidOperationException(
                "Remove the milestone association before changing projects."
            );
        }

        ProjectId = projectId;
    }

    public void RemoveFromProject()
    {
        EnsureAssociationCanChange();

        if (MilestoneId is not null)
        {
            throw new InvalidOperationException(
                "Remove the milestone association before removing the project."
            );
        }

        ProjectId = null;
    }

    public void AssignToMilestone(int milestoneId, int projectId)
    {
        EnsureAssociationCanChange();

        if (milestoneId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(milestoneId));
        }

        if (ProjectId != projectId)
        {
            throw new InvalidOperationException(
                "A quest and its milestone must belong to the same project."
            );
        }

        MilestoneId = milestoneId;
    }

    public void RemoveFromMilestone()
    {
        EnsureAssociationCanChange();
        MilestoneId = null;
    }

    public void Activate()
    {
        if (Status != QuestStatus.Created)
        {
            throw new InvalidOperationException(
                "Only created quests can be activated."
            );
        }

        Status = QuestStatus.Active;
        ActivatedAt = DateTime.Now;
    }

    public void Complete()
    {
        if (Status != QuestStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active quests can be completed."
            );
        }

        Status = QuestStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == QuestStatus.Archived)
        {
            throw new InvalidOperationException(
                "The quest is already archived."
            );
        }

        Status = QuestStatus.Archived;
        ArchivedAt = DateTime.Now;
    }

    private void SetDetails(string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title.Trim();
        Description = description.Trim();
    }

    private void EnsureAssociationCanChange()
    {
        if (Status is QuestStatus.Completed or QuestStatus.Archived)
        {
            throw new InvalidOperationException(
                "Completed or archived quests cannot change associations."
            );
        }
    }

    private void EnsureNotArchived()
    {
        if (Status == QuestStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived quests cannot be changed."
            );
        }
    }
}
