using LevelUp.Domain.Milestones;
using LevelUp.Domain.Attributes;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Quests;

public sealed class QuestService
{
    private readonly List<Quest> quests = [];
    private int nextId = 1;

    public QuestService(IEnumerable<Quest>? quests = null)
    {
        if (quests is null)
        {
            return;
        }

        this.quests.AddRange(quests);

        if (this.quests.Count > 0)
        {
            nextId = this.quests.Max(quest => quest.Id) + 1;
        }
    }

    public Quest CreateQuest(
        string title,
        string description,
        Project? project = null
    ) => CreateQuest(title, description, project, AttributeType.Intelligence);

    public Quest CreateQuest(
        string title,
        string description,
        Project? project,
        AttributeType independentAttribute
    )
    {
        Quest quest = new()
        {
            Id = nextId++
        };

        quest.Configure(title, description, project?.PrimaryAttribute ?? independentAttribute);

        if (project is not null)
        {
            EnsureAssignableProject(project);
            quest.AssignToProject(project.Id);
        }

        quests.Add(quest);
        return quest;
    }

    public IReadOnlyList<Quest> GetAllQuests() => quests.AsReadOnly();

    public Quest? GetQuestById(int id)
    {
        return id <= 0
            ? null
            : quests.FirstOrDefault(quest => quest.Id == id);
    }

    public IReadOnlyList<Quest> GetQuestsByProjectId(int projectId)
    {
        return projectId <= 0
            ? []
            : quests
                .Where(quest => quest.ProjectId == projectId)
                .ToList()
                .AsReadOnly();
    }

    public IReadOnlyList<Quest> GetQuestsByMilestoneId(int milestoneId)
    {
        return milestoneId <= 0
            ? []
            : quests
                .Where(quest => quest.MilestoneId == milestoneId)
                .ToList()
                .AsReadOnly();
    }

    public IReadOnlyList<Quest> GetIndependentQuests()
    {
        return quests
            .Where(quest => quest.ProjectId is null)
            .ToList()
            .AsReadOnly();
    }

    public void UpdateQuest(
        Quest quest,
        string title,
        string description
    )
    {
        EnsureManagedQuest(quest);
        quest.UpdateDetails(title, description);
    }

    public void ActivateQuest(Quest quest)
    {
        EnsureManagedQuest(quest);
        quest.Activate();
    }

    public void CompleteQuest(Quest quest)
    {
        EnsureManagedQuest(quest);
        quest.Complete();
    }

    public Quest? ArchiveQuest(Quest quest)
    {
        EnsureManagedQuest(quest);
        int? projectId = quest.ProjectId;
        int? milestoneId = quest.MilestoneId;
        bool wasActive = quest.Status == QuestStatus.Active;
        quest.Archive();
        return wasActive ? ActivateFirstAvailableQuest(projectId, milestoneId) : null;
    }

    public void AssignQuestToProject(Quest quest, Project project)
    {
        EnsureManagedQuest(quest);
        ArgumentNullException.ThrowIfNull(project);
        EnsureAssignableProject(project);
        quest.AssignToProject(project.Id);
        quest.InheritAttribute(project.PrimaryAttribute);
    }

    public void RemoveQuestFromProject(Quest quest)
    {
        EnsureManagedQuest(quest);
        quest.RemoveFromProject();
    }

    public void AssignQuestToMilestone(Quest quest, Milestone milestone)
    {
        EnsureManagedQuest(quest);
        ArgumentNullException.ThrowIfNull(milestone);

        if (!milestone.CanAcceptQuests)
        {
            throw new InvalidOperationException(
                "Tasks cannot be associated with completed or archived milestones."
            );
        }

        quest.AssignToMilestone(milestone.Id, milestone.ProjectId);
    }

    public void RemoveQuestFromMilestone(Quest quest)
    {
        EnsureManagedQuest(quest);
        quest.RemoveFromMilestone();
    }


    public Quest? ActivateNextQuest(Quest completedQuest)
    {
        EnsureManagedQuest(completedQuest);

        if (completedQuest.Status != QuestStatus.Completed)
        {
            throw new InvalidOperationException(
                "The next task can only be activated after the current task is completed."
            );
        }

        IEnumerable<Quest> sequence = completedQuest.MilestoneId is int milestoneId
            ? quests.Where(quest => quest.MilestoneId == milestoneId)
            : completedQuest.ProjectId is int projectId
                ? quests.Where(quest =>
                    quest.ProjectId == projectId &&
                    quest.MilestoneId is null)
                : quests.Where(quest => quest.ProjectId is null);

        Quest? next = sequence
            .Where(quest =>
                quest.Id > completedQuest.Id &&
                quest.Status == QuestStatus.Created)
            .OrderBy(quest => quest.Id)
            .FirstOrDefault();

        if (next is not null)
        {
            next.Activate();
        }

        return next;
    }

    public Quest? ActivateFirstQuestForMilestone(int milestoneId)
    {
        if (milestoneId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(milestoneId));
        }

        if (quests.Any(quest =>
            quest.MilestoneId == milestoneId &&
            quest.Status == QuestStatus.Active))
        {
            return null;
        }

        Quest? first = quests
            .Where(quest =>
                quest.MilestoneId == milestoneId &&
                quest.Status == QuestStatus.Created)
            .OrderBy(quest => quest.Id)
            .FirstOrDefault();

        if (first is not null)
        {
            first.Activate();
        }

        return first;
    }

    public Quest? ActivateFirstProjectQuest(int projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(projectId));
        }

        if (quests.Any(quest =>
            quest.ProjectId == projectId &&
            quest.Status == QuestStatus.Active))
        {
            return null;
        }

        Quest? first = quests
            .Where(quest =>
                quest.ProjectId == projectId &&
                quest.MilestoneId is null &&
                quest.Status == QuestStatus.Created)
            .OrderBy(quest => quest.Id)
            .FirstOrDefault();

        if (first is not null)
        {
            first.Activate();
        }

        return first;
    }

    public bool DeleteQuest(int id)
    {
        Quest? quest = GetQuestById(id);
        if (quest is null)
        {
            return false;
        }

        int? projectId = quest.ProjectId;
        int? milestoneId = quest.MilestoneId;
        bool shouldReconcile = quest.Status is QuestStatus.Active or QuestStatus.Completed;
        bool removed = quests.Remove(quest);

        if (removed && shouldReconcile)
        {
            ActivateFirstAvailableQuest(projectId, milestoneId);
        }

        return removed;
    }

    public Quest? ActivateFirstAvailableQuest(int? projectId, int? milestoneId)
    {
        IEnumerable<Quest> sequence = milestoneId is int chapterId
            ? quests.Where(quest => quest.MilestoneId == chapterId)
            : projectId is int parentProjectId
                ? quests.Where(quest => quest.ProjectId == parentProjectId && quest.MilestoneId is null)
                : quests.Where(quest => quest.ProjectId is null);

        if (sequence.Any(quest => quest.Status == QuestStatus.Active))
        {
            return null;
        }

        Quest? next = sequence
            .Where(quest => quest.Status == QuestStatus.Created)
            .OrderBy(quest => quest.Id)
            .FirstOrDefault();

        if (next is not null)
        {
            next.Activate();
        }

        return next;
    }

    public bool IsCompleted(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);
        return quest.Status == QuestStatus.Completed;
    }

    private void EnsureManagedQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);

        if (!quests.Any(existingQuest => existingQuest.Id == quest.Id))
        {
            throw new InvalidOperationException(
                "The task is not managed by this service."
            );
        }
    }

    private static void EnsureAssignableProject(Project project)
    {
        if (project.Status is not (ProjectStatus.Created or ProjectStatus.Active))
        {
            throw new InvalidOperationException(
                "Tasks can only be associated with created or active projects."
            );
        }
    }
}
