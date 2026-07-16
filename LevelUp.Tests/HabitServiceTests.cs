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
        Habit habit = service.CreateHabit(
            "Treino",
            "Descrição",
            30,
            AttributeType.Strength
        );

        service.UpdateHabit(
            habit,
            "Treino atualizado",
            "Nova descrição",
            45,
            AttributeType.Vitality
        );

        Assert.Equal("Treino atualizado", habit.Title);
        Assert.Equal("Nova descrição", habit.Description);
        Assert.Equal(45, habit.DurationInMinutes);
        Assert.Equal(AttributeType.Vitality, habit.AttributeType);
    }

    [Fact]
    public void DeleteHabit_ShouldRemoveManagedHabit()
    {
        HabitService service = new();
        Habit habit = service.CreateHabit(
            "Treino",
            "Descrição",
            30,
            AttributeType.Strength
        );

        bool deleted = service.DeleteHabit(habit.Id);

        Assert.True(deleted);
        Assert.Empty(service.GetAllHabits());
    }

    [Fact]
    public void LoadHabits_ShouldContinueIdSequence()
    {
        HabitService service = new();
        service.LoadHabits(
        [
            new Habit
            {
                Id = 7,
                Title = "Existente",
                DurationInMinutes = 20
            }
        ]);

        Habit created = service.CreateHabit(
            "Novo",
            "Descrição",
            30,
            AttributeType.Intelligence
        );

        Assert.Equal(8, created.Id);
    }
}
