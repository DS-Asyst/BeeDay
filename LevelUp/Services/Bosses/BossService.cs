using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;

namespace LevelUp.Services.Bosses;

public sealed class BossService
{
    private readonly List<BossEncounter> bosses = [];
    private int nextId = 1;

    public BossService(IEnumerable<BossEncounter>? bosses = null)
    {
        if (bosses is null)
        {
            return;
        }

        this.bosses.AddRange(bosses);
        if (this.bosses.Count > 0)
        {
            nextId = this.bosses.Max(boss => boss.Id) + 1;
        }
    }

    public BossEncounter Create(
        Project project,
        Milestone milestone,
        string name,
        string description,
        bool isFinalBoss = false
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(milestone);

        if (milestone.ProjectId != project.Id)
        {
            throw new InvalidOperationException(
                "The boss and milestone must belong to the same project."
            );
        }

        if (bosses.Any(boss => boss.MilestoneId == milestone.Id))
        {
            throw new InvalidOperationException(
                "A milestone can have only one boss encounter."
            );
        }

        BossEncounter boss = new() { Id = nextId++ };
        boss.Configure(project.Id, milestone.Id, name, description, isFinalBoss);
        bosses.Add(boss);
        return boss;
    }

    public IReadOnlyList<BossEncounter> GetAll() => bosses.AsReadOnly();

    public IReadOnlyList<BossEncounter> GetByProjectId(int projectId)
    {
        return bosses.Where(boss => boss.ProjectId == projectId).ToList().AsReadOnly();
    }

    public BossEncounter? GetByMilestoneId(int milestoneId)
    {
        return bosses.FirstOrDefault(boss => boss.MilestoneId == milestoneId);
    }

    public bool TryUnlockForMilestoneRequirement(
        Milestone milestone,
        bool requirementsMet
    )
    {
        ArgumentNullException.ThrowIfNull(milestone);
        BossEncounter? boss = GetByMilestoneId(milestone.Id);

        if (!requirementsMet ||
            boss is null ||
            milestone.Status != MilestoneStatus.Active ||
            boss.Status != BossStatus.Locked)
        {
            return false;
        }

        boss.Unlock();
        return true;
    }

    public void Defeat(BossEncounter boss)
    {
        EnsureManaged(boss);
        boss.Defeat();
    }

    public void Archive(BossEncounter boss)
    {
        EnsureManaged(boss);
        boss.Archive();
    }

    public void DeleteByProjectId(int projectId)
    {
        bosses.RemoveAll(boss => boss.ProjectId == projectId);
    }

    public void DeleteByMilestoneId(int milestoneId)
    {
        bosses.RemoveAll(boss => boss.MilestoneId == milestoneId);
    }

    private void EnsureManaged(BossEncounter boss)
    {
        ArgumentNullException.ThrowIfNull(boss);

        if (!bosses.Any(existing => existing.Id == boss.Id))
        {
            throw new InvalidOperationException("The boss is not managed by this service.");
        }
    }
}
