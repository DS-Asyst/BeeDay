using LevelUp.Domain.Attributes;
using LevelUp.Domain.Rewards;
using LevelUp.Domain.Tasks;
using DomainTaskStatus = LevelUp.Domain.Tasks.TaskStatus;

namespace LevelUp.Services.Tasks;

public sealed class TaskService
{
    private readonly List<TaskItem> tasks = []; private int nextId = 1;
    public TaskService(IEnumerable<TaskItem>? items = null) { if (items is not null) { tasks.AddRange(items); if (tasks.Count > 0) nextId = tasks.Max(x => x.Id) + 1; } }
    public TaskItem Create(string title, string description, AttributeType attribute, TaskRecurrence recurrence = TaskRecurrence.Daily, WeekDays repeatOn = WeekDays.EveryDay)
    { var item = new TaskItem { Id = nextId++ }; item.Configure(title, description, attribute, recurrence, repeatOn); tasks.Add(item); return item; }
    public IReadOnlyList<TaskItem> GetAll() => tasks.AsReadOnly();
    public IReadOnlyList<TaskItem> GetForDate(DateTime date) => tasks.Where(x => x.Status == DomainTaskStatus.Active && x.IsScheduledFor(date.DayOfWeek)).ToList().AsReadOnly();
    public TaskItem? GetById(int id) => tasks.FirstOrDefault(x => x.Id == id);
    public Reward Complete(TaskItem item, DateTime? at = null) { Ensure(item); item.Complete(at); return new Reward(1m, item.AttributeType, 1m); }
    public bool Delete(int id) => GetById(id) is { } item && tasks.Remove(item);
    private void Ensure(TaskItem item) { ArgumentNullException.ThrowIfNull(item); if (!tasks.Any(x => x.Id == item.Id)) throw new InvalidOperationException("The task is not managed by this service."); }
}
