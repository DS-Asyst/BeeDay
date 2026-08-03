using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class ActivityAttributeTests
{
    [Theory]
    [InlineData(ActivityAttribute.Strength)]
    [InlineData(ActivityAttribute.Dexterity)]
    [InlineData(ActivityAttribute.Intelligence)]
    [InlineData(ActivityAttribute.Vitality)]
    public void Create_PersistsSupportedAttribute(ActivityAttribute attribute)
    {
        var habit = Habit.Create("Train", null, HabitDirection.Both, HabitDifficulty.Easy, HabitResetCounter.Daily, attribute);
        Assert.Equal(attribute, habit.Attribute);
    }

    [Fact]
    public void SetAttribute_AllowsRemovingSelection()
    {
        var task = RecurringTask.Create("Read", null, TaskRepeat.Daily, ActivityAttribute.Intelligence);
        task.SetAttribute(null);
        Assert.Null(task.Attribute);
    }

    [Fact]
    public void SetAttribute_RejectsUndefinedValue()
    {
        var project = Project.Create("Project", null);
        Assert.Throws<DomainValidationException>(() => project.SetAttribute((ActivityAttribute)999));
    }
}
