using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Habits;

public sealed class Habit
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AttributeType AttributeType { get; set; }
    public HabitDirection Direction { get; set; } = HabitDirection.Positive;
    public int PositiveCount { get; set; }
    public int NegativeCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastScoredAt { get; set; }
    public decimal ExperienceReward => 0.5m;
    public decimal AttributeExperienceReward => 0.5m;

    public bool AllowsPositive => Direction is HabitDirection.Positive or HabitDirection.Both;
    public bool AllowsNegative => Direction is HabitDirection.Negative or HabitDirection.Both;
}
