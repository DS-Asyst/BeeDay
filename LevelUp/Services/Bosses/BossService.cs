using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

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

        this.bosses.AddRange(
            bosses.Where(boss => boss.MilestoneId is null)
        );
        if (this.bosses.Count > 0)
        {
            nextId = this.bosses.Max(boss => boss.Id) + 1;
        }
    }

    public BossEncounter CreateFinalBoss(
        Project project,
        string name,
        string description,
        string achievementPrefix
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        if (GetByProjectId(project.Id) is not null)
        {
            throw new InvalidOperationException("Um projeto pode possuir apenas um chefe final.");
        }

        BossEncounter boss = new() { Id = nextId++ };
        boss.Configure(project.Id, name, description, achievementPrefix);
        bosses.Add(boss);
        return boss;
    }


    public BossEncounter Create(
        Project project,
        Milestone milestone,
        string name,
        string description,
        bool isFinalBoss = false
    )
    {
        ArgumentNullException.ThrowIfNull(milestone);
        BossEncounter boss = new() { Id = nextId++ };
        boss.Configure(project.Id, milestone.Id, name, description, isFinalBoss);
        bosses.Add(boss);
        return boss;
    }

    public bool TryUnlockForMilestoneRequirement(
        Milestone milestone,
        bool requirementsMet
    )
    {
        BossEncounter? boss = GetByMilestoneId(milestone.Id);
        if (!requirementsMet || boss is null || boss.Status != BossStatus.Locked)
        {
            return false;
        }
        boss.Unlock();
        return true;
    }

    public IReadOnlyList<BossEncounter> GetAll() => bosses.AsReadOnly();

    public BossEncounter? GetByProjectId(int projectId)
    {
        return bosses.FirstOrDefault(
            boss => boss.ProjectId == projectId &&
                boss.MilestoneId is null &&
                boss.Status != BossStatus.Archived
        );
    }

    public BossEncounter? GetByMilestoneId(int milestoneId)
    {
        return bosses.FirstOrDefault(boss => boss.MilestoneId == milestoneId);
    }

    public void Update(
        BossEncounter boss,
        string name,
        string description,
        string achievementPrefix
    )
    {
        EnsureManaged(boss);
        boss.UpdateDetails(name, description, achievementPrefix);
    }

    public bool TryUnlockForProject(
        Project project,
        IEnumerable<Quest> quests,
        IEnumerable<Milestone> milestones
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(quests);
        ArgumentNullException.ThrowIfNull(milestones);

        BossEncounter? boss = GetByProjectId(project.Id);
        if (boss is null || boss.Status != BossStatus.Locked || project.Status != ProjectStatus.Active)
        {
            return false;
        }

        List<Quest> validQuests = quests
            .Where(quest => quest.ProjectId == project.Id && quest.Status != QuestStatus.Archived)
            .ToList();
        List<Milestone> validMilestones = milestones
            .Where(item => item.ProjectId == project.Id && item.Status != MilestoneStatus.Archived)
            .ToList();

        bool hasProgressItems = validQuests.Count > 0 || validMilestones.Count > 0;
        bool questsCompleted = validQuests.All(quest => quest.Status == QuestStatus.Completed);
        bool milestonesCompleted = validMilestones.All(item => item.Status == MilestoneStatus.Completed);

        if (!hasProgressItems || !questsCompleted || !milestonesCompleted)
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
            throw new InvalidOperationException("O chefe não é gerenciado por este serviço.");
        }
    }
}
