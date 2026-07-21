using System.Text.Json.Serialization;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Entities;

public sealed class Habit : Activity
{
    [JsonInclude]
    public HabitDirection Direction { get; private set; } = HabitDirection.Both;

    [JsonInclude]
    public HabitDifficulty Difficulty { get; private set; } = HabitDifficulty.Easy;

    [JsonInclude]
    public HabitResetCounter ResetCounter { get; private set; } = HabitResetCounter.Daily;

    [JsonInclude]
    public int PositiveCount { get; private set; }

    [JsonInclude]
    public int NegativeCount { get; private set; }

    public static Habit Create(string title, string? description, HabitDirection direction, HabitDifficulty difficulty, HabitResetCounter resetCounter)
    {
        var habit = new Habit();
        habit.Update(title, description, direction, difficulty, resetCounter);
        return habit;
    }

    public void Update(string title, string? description, HabitDirection direction, HabitDifficulty difficulty, HabitResetCounter resetCounter)
    {
        UpdateDetails(title, description);
        Direction = EnumValidation.Defined(direction, nameof(direction));
        Difficulty = EnumValidation.Defined(difficulty, nameof(difficulty));
        ResetCounter = EnumValidation.Defined(resetCounter, nameof(resetCounter));
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
