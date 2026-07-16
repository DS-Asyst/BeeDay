using System.Text.Json.Serialization;

namespace LevelUp.Domain.Bosses;

public sealed class BossEncounter
{
    public int Id { get; set; }
    public int ProjectId { get; private set; }
    public int MilestoneId { get; private set; }

    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public bool IsFinalBoss { get; private set; }

    [JsonInclude]
    public BossStatus Status { get; private set; } = BossStatus.Locked;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [JsonInclude]
    public DateTime? UnlockedAt { get; private set; }

    [JsonInclude]
    public DateTime? DefeatedAt { get; private set; }

    [JsonInclude]
    public DateTime? ArchivedAt { get; private set; }

    public void Configure(
        int projectId,
        int milestoneId,
        string name,
        string description,
        bool isFinalBoss = false
    )
    {
        if (ProjectId > 0 || MilestoneId > 0)
        {
            throw new InvalidOperationException("O encontro com o chefe já foi configurado.");
        }

        if (projectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(projectId));
        }

        if (milestoneId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(milestoneId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ProjectId = projectId;
        MilestoneId = milestoneId;
        Name = name.Trim();
        Description = description.Trim();
        IsFinalBoss = isFinalBoss;
    }

    public void Unlock()
    {
        if (Status != BossStatus.Locked)
        {
            throw new InvalidOperationException("Apenas chefes bloqueados podem ser desbloqueados.");
        }

        Status = BossStatus.Available;
        UnlockedAt = DateTime.Now;
    }

    public void Defeat()
    {
        if (Status != BossStatus.Available)
        {
            throw new InvalidOperationException("Apenas chefes disponíveis podem ser derrotados.");
        }

        Status = BossStatus.Defeated;
        DefeatedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == BossStatus.Archived)
        {
            throw new InvalidOperationException("O chefe já está arquivado.");
        }

        Status = BossStatus.Archived;
        ArchivedAt = DateTime.Now;
    }
}
