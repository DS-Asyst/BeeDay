
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
            throw new InvalidOperationException("O capítulo já foi configurado.");
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
            throw new InvalidOperationException("Apenas capítulos bloqueados podem ser desbloqueados.");
        }

        Status = MilestoneStatus.Created;
        UnlockedAt = DateTime.Now;
    }

    public void Activate()
    {
        if (Status != MilestoneStatus.Created)
        {
            throw new InvalidOperationException("Apenas capítulos criados podem ser ativados.");
        }

        Status = MilestoneStatus.Active;
        ActivatedAt = DateTime.Now;
    }

    public void Complete()
    {
        if (Status != MilestoneStatus.Active)
        {
            throw new InvalidOperationException("Apenas capítulos ativos podem ser concluídos.");
        }

        Status = MilestoneStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void ClaimReward()
    {
        if (CompletedAt is null)
        {
            throw new InvalidOperationException("Apenas capítulos concluídos podem conceder recompensas.");
        }

        if (!Reward.HasReward)
        {
            throw new InvalidOperationException("Este capítulo não possui recompensa configurada.");
        }

        if (RewardClaimedAt is not null)
        {
            throw new InvalidOperationException("A recompensa deste capítulo já foi resgatada.");
        }

        RewardClaimedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == MilestoneStatus.Archived)
        {
            throw new InvalidOperationException("O capítulo já está arquivado.");
        }

        Status = MilestoneStatus.Archived;
        ArchivedAt = DateTime.Now;
    }

    private void EnsureMutable()
    {
        if (Status is MilestoneStatus.Completed or MilestoneStatus.Archived)
        {
            throw new InvalidOperationException("Capítulos concluídos ou arquivados não podem ser alterados.");
        }
    }
}
