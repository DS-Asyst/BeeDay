using System.Text.Json.Serialization;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;

namespace LevelUp.Domain.Entities;

public sealed class RecurringTask : Activity
{
    [JsonInclude]
    public TaskRepeat Repeat { get; private set; } = TaskRepeat.Daily;

    public static RecurringTask Create(string title, string? description, TaskRepeat repeat)
    {
        var task = new RecurringTask();
        task.Update(title, description, repeat);
        return task;
    }

    public void Update(string title, string? description, TaskRepeat repeat)
    {
        UpdateDetails(title, description);
        Repeat = EnumValidation.Defined(repeat, nameof(repeat));
    }
}
