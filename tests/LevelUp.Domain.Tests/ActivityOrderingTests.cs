using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class ActivityOrderingTests
{
    [Fact]
    public void ReorderTasksChangesPersistedListOrder()
    {
        var data = CreateDataWithUser();
        var first = RecurringTask.Create("First", string.Empty, TaskRepeat.None);
        var second = RecurringTask.Create("Second", string.Empty, TaskRepeat.None);
        var third = RecurringTask.Create("Third", string.Empty, TaskRepeat.None);
        data.AddTask(first);
        data.AddTask(second);
        data.AddTask(third);

        data.ReorderTasks([third.Id, first.Id, second.Id]);

        Assert.Equal([third.Id, first.Id, second.Id], data.Tasks.Select(item => item.Id));
    }

    [Fact]
    public void ReorderVisibleTasksPreservesHiddenItemSlots()
    {
        var data = CreateDataWithUser();
        var visibleFirst = RecurringTask.Create("Visible first", string.Empty, TaskRepeat.None);
        var hidden = RecurringTask.Create("Hidden", string.Empty, TaskRepeat.None);
        var visibleSecond = RecurringTask.Create("Visible second", string.Empty, TaskRepeat.None);
        data.AddTask(visibleFirst);
        data.AddTask(hidden);
        data.AddTask(visibleSecond);

        data.ReorderTasks([visibleSecond.Id, visibleFirst.Id]);

        Assert.Equal([visibleSecond.Id, hidden.Id, visibleFirst.Id], data.Tasks.Select(item => item.Id));
    }

    [Fact]
    public void ReorderRejectsUnknownIdentifier()
    {
        var data = CreateDataWithUser();
        var task = RecurringTask.Create("Task", string.Empty, TaskRepeat.None);
        data.AddTask(task);

        Assert.Throws<ArgumentException>(() => data.ReorderTasks([task.Id, Guid.NewGuid()]));
    }
    private static LevelUpData CreateDataWithUser()
    {
        var data = new LevelUpData();
        data.AddUser(User.Create("Test User", $"test-{Guid.NewGuid():N}@levelup.test"));
        return data;
    }

}
