using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class LevelUpDataTests
{
    [Fact]
    public void NewData_StartsWithoutUser()
    {
        var data = new LevelUpData();

        data.EnsureValidState();

        Assert.Empty(data.Users);
        Assert.Null(data.CurrentUserId);
        Assert.Null(data.CurrentUser);
    }

    [Fact]
    public void ActivityCreation_RequiresCurrentUser()
    {
        var data = new LevelUpData();

        Assert.Throws<InvalidDomainStateException>(() =>
            data.AddHabit(Habit.Create("Study", null, LevelUp.Domain.Enums.HabitDirection.Positive, LevelUp.Domain.Enums.HabitDifficulty.Easy, LevelUp.Domain.Enums.HabitResetCounter.Daily)));
    }
}
