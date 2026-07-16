using System.Text.Json.Serialization;

namespace LevelUp.Domain.Goals;

public sealed class Goal
{
    public int Id { get; set; }

    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public GoalMetric Metric { get; private set; }

    [JsonInclude]
    public decimal TargetValue { get; private set; }

    [JsonInclude]
    public GoalStatus Status { get; private set; } = GoalStatus.Active;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? ArchivedAt { get; private set; }

    public void Configure(string name, string description, GoalMetric metric, decimal targetValue)
    {
        if (!string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("A meta já foi configurada.");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (targetValue <= 0) throw new ArgumentOutOfRangeException(nameof(targetValue));
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Metric = metric;
        TargetValue = targetValue;
    }

    public void UpdateDetails(string name, string description, decimal targetValue)
    {
        if (Status != GoalStatus.Active)
            throw new InvalidOperationException("Apenas metas ativas podem ser editadas.");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (targetValue <= 0) throw new ArgumentOutOfRangeException(nameof(targetValue));
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        TargetValue = targetValue;
    }

    public bool Evaluate(decimal currentValue)
    {
        if (Status != GoalStatus.Active || currentValue < TargetValue) return false;
        Status = GoalStatus.Completed;
        CompletedAt = DateTime.Now;
        return true;
    }

    public void Archive()
    {
        if (Status == GoalStatus.Archived)
            throw new InvalidOperationException("A meta já está arquivada.");
        Status = GoalStatus.Archived;
        ArchivedAt = DateTime.Now;
    }
}
