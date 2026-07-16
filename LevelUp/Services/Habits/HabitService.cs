using LevelUp.Domain.Attributes;
using LevelUp.Domain.Habits;

namespace LevelUp.Services.Habits;

public class HabitService
{
    private readonly List<Habit> habits = [];
    private int nextId = 1;

    public Habit CreateHabit(
        string title,
        string description,
        int durationInMinutes,
        AttributeType attributeType
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (durationInMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationInMinutes),
                "A duração deve ser maior que zero."
            );
        }

        Habit habit = new()
        {
            Id = nextId++,
            Title = title.Trim(),
            Description = description.Trim(),
            DurationInMinutes = durationInMinutes,
            AttributeType = attributeType
        };

        habits.Add(habit);
        return habit;
    }

    public List<Habit> GetAllHabits()
    {
        return habits.ToList();
    }

    public Habit? GetHabitById(int id)
    {
        return habits.FirstOrDefault(habit => habit.Id == id);
    }

    public void UpdateHabit(
        Habit habit,
        string title,
        string description,
        int durationInMinutes,
        AttributeType attributeType
    )
    {
        EnsureManagedHabit(habit);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (durationInMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationInMinutes),
                "A duração deve ser maior que zero."
            );
        }

        habit.Title = title.Trim();
        habit.Description = description.Trim();
        habit.DurationInMinutes = durationInMinutes;
        habit.AttributeType = attributeType;
    }

    public bool DeleteHabit(int id)
    {
        Habit? habit = GetHabitById(id);
        return habit is not null && habits.Remove(habit);
    }

    public decimal CompleteHabit(Habit habit)
    {
        EnsureManagedHabit(habit);
        habit.TimesCompleted++;
        return habit.ExperienceReward;
    }

    public void LoadHabits(List<Habit> loadedHabits)
    {
        ArgumentNullException.ThrowIfNull(loadedHabits);

        habits.Clear();
        habits.AddRange(loadedHabits);
        nextId = habits.Count == 0
            ? 1
            : habits.Max(habit => habit.Id) + 1;
    }

    private void EnsureManagedHabit(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);

        if (!habits.Any(existing => existing.Id == habit.Id))
        {
            throw new InvalidOperationException(
                "O treinamento não é gerenciado por este serviço."
            );
        }
    }
}
