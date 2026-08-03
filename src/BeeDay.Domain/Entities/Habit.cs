using BeeDay.Domain.Common;
using BeeDay.Domain.Enums;

namespace BeeDay.Domain.Entities;

public sealed class Habit : Activity
{
    public HabitDirection Direction { get; private set; } = HabitDirection.Both;

    public HabitDifficulty Difficulty { get; private set; } = HabitDifficulty.Easy;

    public HabitResetCounter ResetCounter { get; private set; } = HabitResetCounter.Daily;

    public int PositiveCount { get; private set; }

    public int NegativeCount { get; private set; }

    public static Habit Create(string title, string? description, HabitDirection direction, HabitDifficulty difficulty, HabitResetCounter resetCounter, ActivityAttribute? attribute = null)
    {
        var habit = new Habit();
        habit.Update(title, description, direction, difficulty, resetCounter, attribute);
        return habit;
    }

    public void Update(string title, string? description, HabitDirection direction, HabitDifficulty difficulty, HabitResetCounter resetCounter, ActivityAttribute? attribute = null)
    {
        UpdateDetails(title, description);
        Direction = EnumValidation.Defined(direction, nameof(direction));
        Difficulty = EnumValidation.Defined(difficulty, nameof(difficulty));
        ResetCounter = EnumValidation.Defined(resetCounter, nameof(resetCounter));
        SetAttribute(attribute);
    }

    public void RegisterPositive()
    {
        if (Direction == HabitDirection.Negative)
        {
            return;
        }
        PositiveCount = checked(PositiveCount + 1);
        Touch();
    }

    public void RegisterNegative()
    {
        if (Direction == HabitDirection.Positive)
        {
            return;
        }
        NegativeCount = checked(NegativeCount + 1);
        Touch();
    }
}
