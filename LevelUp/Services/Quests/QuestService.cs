using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using QuestModel = LevelUp.Domain.Quests.Quest;

namespace LevelUp.Services.Quests;

public sealed class QuestService
{
    private readonly List<Quest> quests = [];

    private int nextId = 1;

    public QuestService(
        IEnumerable<Quest>? quests = null
    )
    {
        if (quests is null)
        {
            return;
        }

        this.quests.AddRange(quests);

        if (this.quests.Count > 0)
        {
            nextId = this.quests.Max(
                quest => quest.Id
            ) + 1;
        }
    }

    public Quest CreateQuest(
    string title,
    string description,
    Project? project = null
)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Quest quest = new()
        {
            Id = nextId++,
            ProjectId = project?.Id
        };

        quest.Configure(
            title,
            description
        );

        quests.Add(quest);

        return quest;
    }

    public IReadOnlyList<QuestModel> GetAllQuests()
    {
        return quests.AsReadOnly();
    }

    public Quest? GetQuestById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return quests.FirstOrDefault(
            quest => quest.Id == id
        );
    }

    public IReadOnlyList<QuestModel> GetQuestsByProjectId(
        int projectId
    )
    {
        if (projectId <= 0)
        {
            return [];
        }

        return quests
            .Where(quest =>
                quest.ProjectId == projectId
            )
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<QuestModel> GetIndependentQuests()
    {
        return quests
            .Where(quest =>
                quest.ProjectId is null
            )
            .ToList()
            .AsReadOnly();
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

    public void AssignQuestToProject(
        Quest quest,
        Project project
    )
    {
        EnsureManagedQuest(quest);
        ArgumentNullException.ThrowIfNull(project);

        quest.ProjectId = project.Id;
    }

    public void RemoveQuestFromProject(Quest quest)
    {
        EnsureManagedQuest(quest);

        quest.ProjectId = null;
    }

    public bool DeleteQuest(int id)
    {
        Quest? quest = GetQuestById(id);

        if (quest is null)
        {
            return false;
        }

        return quests.Remove(quest);
    }

    public bool IsCompleted(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);

        return quest.Status == QuestStatus.Completed;
    }

    private void EnsureManagedQuest(Quest quest)
    {
        ArgumentNullException.ThrowIfNull(quest);

        bool questExists = quests.Any(
            existingQuest =>
                existingQuest.Id == quest.Id
        );

        if (!questExists)
        {
            throw new InvalidOperationException(
                "The quest is not managed by this service."
            );
        }
    }

    public void UpdateQuest(
        Quest quest,
        string title,
        string description
    )
    {
        EnsureManagedQuest(quest);

        quest.UpdateDetails(
            title,
            description
        );
    }
}
