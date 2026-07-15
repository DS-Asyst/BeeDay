using System.Text.Json.Serialization;

namespace LevelUp.Domain.Quests;

public sealed class Quest
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int? ProjectId { get; set; }

    [JsonInclude]
    public QuestStatus Status { get; private set; }
        = QuestStatus.Created;

    public DateTime CreatedAt { get; init; }
        = DateTime.Now;

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? ArchivedAt { get; private set; }

    public void Activate()
    {
        if (Status != QuestStatus.Created)
        {
            throw new InvalidOperationException(
                "Only created quests can be activated."
            );
        }

        Status = QuestStatus.Active;
    }

    public void Complete()
    {
        if (Status != QuestStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active quests can be completed."
            );
        }

        Status = QuestStatus.Completed;
        CompletedAt = DateTime.Now;
    }

    public void Archive()
    {
        if (Status == QuestStatus.Archived)
        {
            throw new InvalidOperationException(
                "The quest is already archived."
            );
        }

        Status = QuestStatus.Archived;
        ArchivedAt = DateTime.Now;
    }
}