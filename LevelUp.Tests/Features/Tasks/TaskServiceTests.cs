using LevelUp.Domain.Attributes;
using LevelUp.Domain.Tasks;
using LevelUp.Services.Tasks;
using Xunit;

namespace LevelUp.Tests;

public sealed class TaskServiceTests
{
    [Fact]
    public void WeeklyTask_ShouldOnlyAppearOnSelectedDays()
    {
        TaskService service = new();
        service.Create("Study", "", AttributeType.Intelligence, TaskRecurrence.Weekly, WeekDays.Monday | WeekDays.Wednesday);
        Assert.Single(service.GetForDate(new DateTime(2026, 7, 20)));
        Assert.Empty(service.GetForDate(new DateTime(2026, 7, 21)));
    }
}
