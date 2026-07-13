namespace LevelUp.Models;

public class Habit
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DurationInMinutes { get; set; }

    public AttributeType AttributeType { get; set; }

    public int TimesCompleted { get; set; }

    public decimal ExperiencePerMinute { get; set; } = 0.1m;

    public decimal AttributeExperiencePerMinute { get; set; } = 0.05m;

    public decimal ExperienceReward =>
        DurationInMinutes * ExperiencePerMinute;

    public decimal AttributeExperienceReward =>
        DurationInMinutes * AttributeExperiencePerMinute;
}