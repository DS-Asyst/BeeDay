using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class HabitTests
{
    [Fact]
    public void RegisterPositive_IncrementsPositiveCounter()
    {
        var habit = Habit.Create("Study", null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily);

        habit.RegisterPositive();

        Assert.Equal(1, habit.PositiveCount);
    }

    [Fact]
    public void RegisterPositive_DoesNotChangeNegativeOnlyHabit()
    {
        var habit = Habit.Create("Avoid sugar", null, HabitDirection.Negative, HabitDifficulty.Medium, HabitResetCounter.Daily);

        habit.RegisterPositive();

        Assert.Equal(0, habit.PositiveCount);
    }

    [Fact]
    public void Create_RejectsEmptyTitle()
    {
        Assert.Throws<DomainValidationException>(() =>
            Habit.Create(" ", string.Empty, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily));
    }

    [Fact]
    public void Create_RejectsUndefinedDirection()
    {
        Assert.Throws<DomainValidationException>(() =>
            Habit.Create("Study", null, (HabitDirection)999, HabitDifficulty.Easy, HabitResetCounter.Daily));
    }
}
