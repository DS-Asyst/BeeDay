using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Entities;

public sealed class RecurringTask : Activity
{
    public TaskRepeat Repeat { get; private set; } = TaskRepeat.Daily;

    public static RecurringTask Create(string title, string? description, TaskRepeat repeat, ActivityAttribute? attribute = null)
    {
        var task = new RecurringTask();
        task.Update(title, description, repeat, attribute);
        return task;
    }

    public void Update(string title, string? description, TaskRepeat repeat, ActivityAttribute? attribute = null)
    {
        UpdateDetails(title, description);
        Repeat = EnumValidation.Defined(repeat, nameof(repeat));
        SetAttribute(attribute);
    }
}
