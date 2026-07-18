using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Milestones;

public sealed class MilestoneService
{
    private readonly List<Milestone> milestones = [];
    private int nextId = 1;

    public MilestoneService(IEnumerable<Milestone>? milestones = null)
    {
        if (milestones is null)
        {
            return;
        }

        this.milestones.AddRange(milestones);
        if (this.milestones.Count > 0)
        {
            nextId = this.milestones.Max(milestone => milestone.Id) + 1;
        }
    }

    public Milestone CreateMilestone(
        Project project,
        string title,
        string description,
        int order,
        int requiredCompletedQuests = 0,
        MilestoneReward? reward = null
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        EnsureProjectAcceptsMilestones(project);

        if (GetByProjectId(project.Id).Any(item => item.Order == order))
        {
            throw new InvalidOperationException(
                "A project cannot contain two milestones with the same order."
            );
        }

        bool initiallyLocked = GetByProjectId(project.Id).Count > 0;

        Milestone milestone = new() { Id = nextId++ };
        milestone.Configure(
            project.Id,
            title,
            description,
            order,
            requiredCompletedQuests,
            reward,
            initiallyLocked
        );

        milestones.Add(milestone);
        return milestone;
    }

    public IReadOnlyList<Milestone> GetAll() => milestones.AsReadOnly();

    public IReadOnlyList<Milestone> GetByProjectId(int projectId)
    {
        return milestones
            .Where(milestone => milestone.ProjectId == projectId)
            .OrderBy(milestone => milestone.Order)
            .ToList()
            .AsReadOnly();
    }

    public Milestone? GetById(int id)
    {
        return id <= 0
            ? null
            : milestones.FirstOrDefault(milestone => milestone.Id == id);
    }

    public void Update(Milestone milestone, string title, string description)
    {
        EnsureManaged(milestone);
        milestone.UpdateDetails(title, description);
    }

    public bool TryActivateFirst(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (project.Status != ProjectStatus.Active ||
            GetByProjectId(project.Id).Any(item => item.Status == MilestoneStatus.Active))
        {
            return false;
        }

        Milestone? first = GetByProjectId(project.Id)
            .FirstOrDefault(item => item.Status is MilestoneStatus.Created or MilestoneStatus.Locked);

        if (first is null)
        {
            return false;
        }

        if (first.Status == MilestoneStatus.Locked)
        {
            first.Unlock();
        }

        first.Activate();
        return true;
    }

    public void Activate(Milestone milestone)
    {
        EnsureManaged(milestone);

        if (GetByProjectId(milestone.ProjectId)
            .Any(item => item.Id != milestone.Id && item.Status == MilestoneStatus.Active))
        {
            throw new InvalidOperationException(
                "Only one milestone can be active per project."
            );
        }

        milestone.Activate();
    }

    public decimal CalculateProgress(Milestone milestone, IEnumerable<Quest> quests)
    {
        List<Quest> relevant = GetProgressQuests(milestone, quests);
        if (relevant.Count == 0)
        {
            return 0m;
        }

        int completed = relevant.Count(quest => quest.Status == QuestStatus.Completed);
        int target = milestone.RequiredCompletedQuests > 0
            ? milestone.RequiredCompletedQuests
            : relevant.Count;

        return Math.Min(100m, Math.Round(completed * 100m / target, 2));
    }

    public bool HasMetRequirements(Milestone milestone, IEnumerable<Quest> quests)
    {
        List<Quest> relevant = GetProgressQuests(milestone, quests);
        if (relevant.Count == 0)
        {
            return false;
        }

        int completed = relevant.Count(quest => quest.Status == QuestStatus.Completed);
        int required = milestone.RequiredCompletedQuests > 0
            ? milestone.RequiredCompletedQuests
            : relevant.Count;

        return completed >= required;
    }

    public bool TryComplete(Milestone milestone, IEnumerable<Quest> quests)
    {
        EnsureManaged(milestone);

        if (milestone.Status != MilestoneStatus.Active)
        {
            return false;
        }

        if (!HasMetRequirements(milestone, quests))
        {
            return false;
        }

        milestone.Complete();
        return true;
    }

    public void CompleteManually(Milestone milestone, IEnumerable<Quest> quests)
    {
        EnsureManaged(milestone);

        if (GetProgressQuests(milestone, quests).Count > 0)
        {
            throw new InvalidOperationException(
                "Milestones with tasks are completed automatically."
            );
        }

        milestone.Complete();
    }

    public Milestone? UnlockAndActivateNext(Milestone completed)
    {
        EnsureManaged(completed);

        Milestone? next = GetByProjectId(completed.ProjectId)
            .FirstOrDefault(item => item.Order > completed.Order && item.Status == MilestoneStatus.Locked);

        if (next is null)
        {
            return null;
        }

        next.Unlock();
        next.Activate();
        return next;
    }

    public void Archive(Milestone milestone)
    {
        EnsureManaged(milestone);
        milestone.Archive();
    }

    public bool Delete(int id)
    {
        Milestone? milestone = GetById(id);
        if (milestone is null)
        {
            return false;
        }

        if (milestone.Status is MilestoneStatus.Completed or MilestoneStatus.Archived)
        {
            throw new InvalidOperationException(
                "Completed or archived milestones are part of the project history and cannot be deleted."
            );
        }

        return milestones.Remove(milestone);
    }

    public bool AreAllCompleted(int projectId)
    {
        List<Milestone> valid = GetByProjectId(projectId)
            .Where(item => item.Status != MilestoneStatus.Archived)
            .ToList();

        return valid.Count == 0 || valid.All(item => item.Status == MilestoneStatus.Completed);
    }

    private List<Quest> GetProgressQuests(Milestone milestone, IEnumerable<Quest> quests)
    {
        EnsureManaged(milestone);
        ArgumentNullException.ThrowIfNull(quests);

        return quests
            .Where(quest =>
                quest.MilestoneId == milestone.Id &&
                quest.Status != QuestStatus.Archived)
            .ToList();
    }

    private void EnsureManaged(Milestone milestone)
    {
        ArgumentNullException.ThrowIfNull(milestone);

        if (!milestones.Any(existing => existing.Id == milestone.Id))
        {
            throw new InvalidOperationException(
                "The milestone is not managed by this service."
            );
        }
    }

    private static void EnsureProjectAcceptsMilestones(Project project)
    {
        if (project.Status is ProjectStatus.Completed or ProjectStatus.Archived)
        {
            throw new InvalidOperationException(
                "Completed or archived projects cannot receive milestones."
            );
        }
    }
}
