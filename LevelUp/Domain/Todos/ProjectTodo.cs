using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Todos;

public sealed class ProjectTodo
{
    public int Id { get; set; }
    public int ProjectId { get; private set; }
    public int? MilestoneId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AttributeType AttributeType { get; private set; } = AttributeType.Intelligence;
    public TodoStatus Status { get; private set; } = TodoStatus.Created;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public void Configure(int projectId, int? milestoneId, string title, string description, AttributeType attribute)
    {
        if (projectId <= 0) throw new ArgumentOutOfRangeException(nameof(projectId));
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ProjectId = projectId; MilestoneId = milestoneId; Title = title.Trim(); Description = description.Trim(); AttributeType = attribute;
    }
    public void Activate() { if (Status != TodoStatus.Created) throw new InvalidOperationException("Only created to-dos can be activated."); Status = TodoStatus.Active; ActivatedAt = DateTime.Now; }
    public void Complete() { if (Status != TodoStatus.Active) throw new InvalidOperationException("Only active to-dos can be completed."); Status = TodoStatus.Completed; CompletedAt = DateTime.Now; }
    public void Update(string title, string description) { if (Status == TodoStatus.Completed) throw new InvalidOperationException("Completed to-dos cannot be changed."); ArgumentException.ThrowIfNullOrWhiteSpace(title); Title = title.Trim(); Description = description.Trim(); }
}
