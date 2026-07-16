using LevelUp.Domain.Milestones;
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
    )
    {
        Quest quest = new()
        {
            Id = nextId++
        };

        quest.Configure(title, description);

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

    public void ArchiveQuest(Quest quest)
    {
        EnsureManagedQuest(quest);
        quest.Archive();
    }

    public void AssignQuestToProject(Quest quest, Project project)
    {
        EnsureManagedQuest(quest);
        ArgumentNullException.ThrowIfNull(project);
        EnsureAssignableProject(project);
        quest.AssignToProject(project.Id);
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
                "Missões só podem ser associadas a capítulos criados ou ativos."
            );
        }

        quest.AssignToMilestone(milestone.Id, milestone.ProjectId);
    }

    public void RemoveQuestFromMilestone(Quest quest)
    {
        EnsureManagedQuest(quest);
        quest.RemoveFromMilestone();
    }

    public bool DeleteQuest(int id)
    {
        Quest? quest = GetQuestById(id);
        return quest is not null && quests.Remove(quest);
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
                "A missão não é gerenciada por este serviço."
            );
        }
    }

    private static void EnsureAssignableProject(Project project)
    {
        if (project.Status is not (ProjectStatus.Created or ProjectStatus.Active))
        {
            throw new InvalidOperationException(
                "Missões só podem ser associadas a projetos criados ou ativos."
            );
        }
    }
}
