
namespace LevelUp.Domain.Milestones;

public sealed class Milestone
{
    public int Id { get; set; }
    public int ProjectId { get; private set; }

    public int Order { get; private set; }

    public int RequiredCompletedQuests { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public MilestoneReward Reward { get; private set; } = new();

    public MilestoneStatus Status { get; private set; } = MilestoneStatus.Created;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? UnlockedAt { get; private set; }

    public DateTime? ActivatedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public DateTime? RewardClaimedAt { get; private set; }

    public bool IsLocked => Status == MilestoneStatus.Locked;
    public bool CanAcceptQuests =>
    Status is MilestoneStatus.Locked
        or MilestoneStatus.Created
        or MilestoneStatus.Active;

    public void Configure(
        int projectId,
        string title,
        string description,
        int order = 1,
        int requiredCompletedQuests = 0,
        MilestoneReward? reward = null,
        bool initiallyLocked = false
    )
    {
        if (ProjectId > 0)
        {
            throw new InvalidOperationException("The milestone has already been configured.");
        }

        if (projectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(projectId));
        }

        if (order <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order));
        }

        if (requiredCompletedQuests < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredCompletedQuests));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        ProjectId = projectId;
        Order = order;
        RequiredCompletedQuests = requiredCompletedQuests;
        Title = title.Trim();
        Description = description.Trim();
        Reward = reward ?? new MilestoneReward();
        Status = initiallyLocked ? MilestoneStatus.Locked : MilestoneStatus.Created;
    }

    public void UpdateDetails(string title, string description)
    {
        EnsureMutable();
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
        Description = description.Trim();
    }

    public void Unlock()
    {
        if (Status != MilestoneStatus.Locked)
        {
            throw new InvalidOperationException("Only locked milestones can be unlocked.");
        }

        Status = MilestoneStatus.Created;
        UnlockedAt = DateTime.Now;
    }

    public void Activate()
    {
        if (Status != MilestoneStatus.Created)
        {
            throw new InvalidOperationException("Only created milestones can be activated.");
        }

        Status = MilestoneStatus.Active;
        ActivatedAt = DateTime.Now;
    }

    public void Complete()
    {
        if (Status != MilestoneStatus.Active)
        {
            throw new InvalidOperationException("Only active milestones can be completed.");
        }

        Status = MilestoneStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void ClaimReward()
    {
        if (CompletedAt is null)
        {
            throw new InvalidOperationException("Only completed milestones can grant rewards.");
        }

        if (!Reward.HasReward)
        {
            throw new InvalidOperationException("This milestone has no configured reward.");
        }

        if (RewardClaimedAt is not null)
        {
            throw new InvalidOperationException("This milestone reward has already been claimed.");
        }

        RewardClaimedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == MilestoneStatus.Archived)
        {
            throw new InvalidOperationException("The milestone is already archived.");
        }

        Status = MilestoneStatus.Archived;
        ArchivedAt = DateTime.Now;
    }

    private void EnsureMutable()
    {
        if (Status is MilestoneStatus.Completed or MilestoneStatus.Archived)
        {
            throw new InvalidOperationException("Completed or archived milestones cannot be changed.");
        }
    }
}
