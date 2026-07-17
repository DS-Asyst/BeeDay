
namespace LevelUp.Domain.Bosses;

public sealed class BossEncounter
{
    public int Id { get; set; }

    public int ProjectId { get; private set; }

    // Mantido para leitura de saves antigos. Novos chefes pertencem ao Projeto.
    public int? MilestoneId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string AchievementPrefix { get; private set; } = string.Empty;

    public bool IsFinalBoss { get; private set; } = true;

    public BossStatus Status { get; private set; } = BossStatus.Locked;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? UnlockedAt { get; private set; }

    public DateTime? DefeatedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public void Configure(
        int projectId,
        string name,
        string description,
        string achievementPrefix
    )
    {
        if (ProjectId > 0)
        {
            throw new InvalidOperationException("O encontro com o chefe já foi configurado.");
        }

        if (projectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(projectId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementPrefix);

        ProjectId = projectId;
        MilestoneId = null;
        Name = name.Trim();
        Description = description.Trim();
        AchievementPrefix = achievementPrefix.Trim();
        IsFinalBoss = true;
    }


    public void Configure(
        int projectId,
        int milestoneId,
        string name,
        string description,
        bool isFinalBoss = false
    )
    {
        Configure(projectId, name, description, "Especialista em");
        MilestoneId = milestoneId;
        IsFinalBoss = isFinalBoss;
    }

    public void UpdateDetails(
        string name,
        string description,
        string achievementPrefix
    )
    {
        if (Status is BossStatus.Defeated or BossStatus.Archived)
        {
            throw new InvalidOperationException("Chefes derrotados ou arquivados não podem ser alterados.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementPrefix);
        Name = name.Trim();
        Description = description.Trim();
        AchievementPrefix = achievementPrefix.Trim();
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
