using LevelUp.Domain.Attributes;
using LevelUp.Domain.Habits;
using LevelUp.Services.Habits;
using Xunit;

namespace LevelUp.Tests;

public sealed class HabitServiceTests
{
    [Fact]
    public void UpdateHabit_ShouldChangeEditableData()
    {
        HabitService service = new();
        Habit habit = service.CreateHabit("Read", "Read technical material", AttributeType.Intelligence, HabitDirection.Both);
        service.UpdateHabit(habit, "Read daily", "Updated", AttributeType.Intelligence, HabitDirection.Positive);
        Assert.Equal("Read daily", habit.Title);
        Assert.Equal(HabitDirection.Positive, habit.Direction);
    }

    [Fact]
    public void ScorePositive_ShouldIncrementPositiveCounter()
    {
        HabitService service = new();
        Habit habit = service.CreateHabit("Exercise", "", AttributeType.Strength);
        service.ScorePositive(habit);
        Assert.Equal(1, habit.PositiveCount);
    }

    [Fact]
    public void NegativeOnlyHabit_ShouldRejectPositiveScore()
    {
        HabitService service = new();
        Habit habit = service.CreateHabit("Avoid sugar", "", AttributeType.Vitality, HabitDirection.Negative);
        Assert.Throws<InvalidOperationException>(() => service.ScorePositive(habit));
    }

    [Fact]
    public void LoadHabits_ShouldContinueIdSequence()
    {
        HabitService service = new([new Habit { Id = 7, Title = "Existing" }]);
        Habit created = service.CreateHabit("New", "", AttributeType.Intelligence);
        Assert.Equal(8, created.Id);
    }
}
