using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Tasks;

public sealed class TaskItem
{
    public int Id { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AttributeType AttributeType { get; private set; } = AttributeType.Intelligence;
    public TaskRecurrence Recurrence { get; private set; } = TaskRecurrence.Daily;
    public WeekDays RepeatOn { get; private set; } = WeekDays.EveryDay;
    public TaskStatus Status { get; private set; } = TaskStatus.Active;
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime? LastCompletedAt { get; private set; }
    public int CompletionCount { get; private set; }

    public void Configure(string title, string description, AttributeType attributeType, TaskRecurrence recurrence, WeekDays repeatOn)
    {
        SetDetails(title, description); AttributeType = attributeType; SetSchedule(recurrence, repeatOn);
    }
    public void Update(string title, string description, AttributeType attributeType, TaskRecurrence recurrence, WeekDays repeatOn)
    {
        SetDetails(title, description); AttributeType = attributeType; SetSchedule(recurrence, repeatOn);
    }
    public void Complete(DateTime? completedAt = null)
    {
        if (Status != TaskStatus.Active) throw new InvalidOperationException("Only active tasks can be completed.");
        CompletionCount++; LastCompletedAt = completedAt ?? DateTime.Now;
    }
    public void Pause() => Status = TaskStatus.Paused;
    public void Resume() => Status = TaskStatus.Active;
    public bool IsScheduledFor(DayOfWeek day) => Recurrence == TaskRecurrence.Daily || RepeatOn.HasFlag(ToWeekDay(day));

    private void SetDetails(string title,string description) { ArgumentException.ThrowIfNullOrWhiteSpace(title); Title=title.Trim(); Description=description.Trim(); }
    private void SetSchedule(TaskRecurrence recurrence, WeekDays repeatOn)
    {
        if (recurrence == TaskRecurrence.Weekly && repeatOn == WeekDays.None) throw new ArgumentException("Weekly tasks must repeat on at least one day.", nameof(repeatOn));
        Recurrence=recurrence; RepeatOn=recurrence==TaskRecurrence.Daily?WeekDays.EveryDay:repeatOn;
    }
    private static WeekDays ToWeekDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday=>WeekDays.Monday, DayOfWeek.Tuesday=>WeekDays.Tuesday, DayOfWeek.Wednesday=>WeekDays.Wednesday,
        DayOfWeek.Thursday=>WeekDays.Thursday, DayOfWeek.Friday=>WeekDays.Friday, DayOfWeek.Saturday=>WeekDays.Saturday,
        DayOfWeek.Sunday=>WeekDays.Sunday, _=>WeekDays.None
    };
}
