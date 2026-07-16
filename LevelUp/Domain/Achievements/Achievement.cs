using System.Text.Json.Serialization;

namespace LevelUp.Domain.Achievements;

public sealed class Achievement
{
    public int Id { get; set; }

    [JsonInclude]
    public string Code { get; private set; } = string.Empty;

    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public AchievementCategory Category { get; private set; }

    [JsonInclude]
    public int? SourceId { get; private set; }

    [JsonInclude]
    public AchievementStatus Status { get; private set; } = AchievementStatus.Locked;

    [JsonInclude]
    public DateTime? UnlockedAt { get; private set; }

    public void Configure(
        string code,
        string name,
        string description,
        AchievementCategory category,
        int? sourceId = null
    )
    {
        if (!string.IsNullOrWhiteSpace(Code))
        {
            throw new InvalidOperationException("A conquista já foi configurada.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code.Trim();
        Name = name.Trim();
        Description = description.Trim();
        Category = category;
        SourceId = sourceId;
    }

    public void Unlock()
    {
        if (Status == AchievementStatus.Unlocked)
        {
            return;
        }

        Status = AchievementStatus.Unlocked;
        UnlockedAt = DateTime.Now;
    }
}
