using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Quests;

public sealed class Quest
{
    public int Id { get; set; }

    public int? ProjectId { get; private set; }

    public int? MilestoneId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public AttributeType AttributeType { get; private set; } = AttributeType.Intelligence;

    public QuestStatus Status { get; private set; } = QuestStatus.Created;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? ActivatedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public void Configure(string title, string description)
        => Configure(title, description, AttributeType.Intelligence);

    public void Configure(string title, string description, AttributeType attributeType)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException(
                "The task has already been configured."
            );
        }

        AttributeType = attributeType;
        SetDetails(title, description);
    }

    public void SetIndependentAttribute(AttributeType attributeType)
    {
        EnsureAssociationCanChange();
        if (ProjectId is not null)
        {
            throw new InvalidOperationException("Only independent tasks can select an attribute manually.");
        }
        AttributeType = attributeType;
    }

    public void InheritAttribute(AttributeType attributeType)
    {
        EnsureAssociationCanChange();
        AttributeType = attributeType;
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
                "Remove the milestone association before changing the project."
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
                "The task and its milestone must belong to the same project."
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
                "Only created tasks can be activated."
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
                "Only active tasks can be completed."
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
                "The task is already archived."
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
                "Completed or archived tasks cannot change associations."
            );
        }
    }

    private void EnsureNotArchived()
    {
        if (Status == QuestStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived tasks cannot be changed."
            );
        }
    }
}
