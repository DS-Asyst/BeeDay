using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Habits;


public class Habit
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Mantido apenas para compatibilidade com saves anteriores; não influencia recompensas.
    public int DurationInMinutes { get; set; }

    public AttributeType AttributeType { get; set; }

    public int TimesCompleted { get; set; }

    public decimal ExperienceReward => 0.5m;

    public decimal AttributeExperienceReward => 0.5m;
}
