using LevelUp.Domain.Attributes;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Rewards;

namespace LevelUp.Services.Habits;

public sealed class HabitService
{
    private readonly List<Habit> habits = [];
    private int nextId = 1;

    public HabitService(IEnumerable<Habit>? habits = null)
    {
        if (habits is not null) LoadHabits(habits.ToList());
    }

    public Habit CreateHabit(string title, string description, AttributeType attributeType, HabitDirection direction = HabitDirection.Positive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Habit habit = new()
        {
            Id = nextId++,
            Title = title.Trim(),
            Description = description.Trim(),
            AttributeType = attributeType,
            Direction = direction,
            CreatedAt = DateTime.Now
        };
        habits.Add(habit);
        return habit;
    }

    public IReadOnlyList<Habit> GetAllHabits() => habits.AsReadOnly();
    public Habit? GetHabitById(int id) => habits.FirstOrDefault(x => x.Id == id);

    public void UpdateHabit(Habit habit, string title, string description, AttributeType attributeType, HabitDirection direction)
    {
        EnsureManagedHabit(habit);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        habit.Title = title.Trim(); habit.Description = description.Trim();
        habit.AttributeType = attributeType; habit.Direction = direction;
    }

    public bool DeleteHabit(int id) => GetHabitById(id) is { } habit && habits.Remove(habit);

    public Reward ScorePositive(Habit habit)
    {
        EnsureManagedHabit(habit);
        if (!habit.AllowsPositive) throw new InvalidOperationException("This habit does not allow positive scoring.");
        habit.PositiveCount++; habit.LastScoredAt = DateTime.Now;
        return new Reward(HabitExperienceReward, habit.AttributeType, HabitAttributeExperienceReward);
    }

    public void ScoreNegative(Habit habit)
    {
        EnsureManagedHabit(habit);
        if (!habit.AllowsNegative) throw new InvalidOperationException("This habit does not allow negative scoring.");
        habit.NegativeCount++; habit.LastScoredAt = DateTime.Now;
    }

    [Obsolete("Use ScorePositive.")]
    public Reward CompleteHabit(Habit habit) => ScorePositive(habit);

    public const decimal HabitExperienceReward = 0.5m;
    public const decimal HabitAttributeExperienceReward = 0.5m;

    public void LoadHabits(List<Habit> loadedHabits)
    {
        ArgumentNullException.ThrowIfNull(loadedHabits);
        habits.Clear(); habits.AddRange(loadedHabits);
        nextId = habits.Count == 0 ? 1 : habits.Max(x => x.Id) + 1;
    }

    private void EnsureManagedHabit(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);
        if (!habits.Any(x => x.Id == habit.Id))
            throw new InvalidOperationException("The habit is not managed by this service.");
    }
}
