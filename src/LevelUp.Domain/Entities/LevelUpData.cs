using System.Text.Json.Serialization;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class LevelUpData
{
    [JsonInclude]
    public int SchemaVersion { get; private set; } = 1;

    [JsonInclude]
    public Profile? Profile { get; private set; }

    [JsonInclude]
    public List<Habit> Habits { get; private set; } = [];

    [JsonInclude]
    public List<RecurringTask> Tasks { get; private set; } = [];

    [JsonInclude]
    public List<Todo> Todos { get; private set; } = [];

    [JsonInclude]
    public List<Project> Projects { get; private set; } = [];

    public void SetProfile(Profile profile) => Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    public void AddHabit(Habit habit) => Habits.Add(habit ?? throw new ArgumentNullException(nameof(habit)));
    public void AddTask(RecurringTask task) => Tasks.Add(task ?? throw new ArgumentNullException(nameof(task)));
    public void AddTodo(Todo todo) => Todos.Add(todo ?? throw new ArgumentNullException(nameof(todo)));
    public void AddProject(Project project) => Projects.Add(project ?? throw new ArgumentNullException(nameof(project)));

    public void ReorderHabits(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Habits, orderedIds);
    public void ReorderTasks(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Tasks, orderedIds);
    public void ReorderTodos(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Todos, orderedIds);
    public void ReorderProjects(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Projects, orderedIds);

    public void EnsureValidState()
    {
        SchemaVersion = Math.Max(1, SchemaVersion);
        Habits ??= [];
        Tasks ??= [];
        Todos ??= [];
        Projects ??= [];

        EnsureUniqueIds(Habits);
        EnsureUniqueIds(Tasks);
        EnsureUniqueIds(Todos);
        EnsureUniqueIds(Projects);
    }

    private static void ReorderVisibleItems<T>(List<T> items, IReadOnlyList<Guid> orderedIds)
        where T : Activity
    {
        ArgumentNullException.ThrowIfNull(orderedIds);

        if (orderedIds.Count < 2)
        {
            return;
        }

        var requestedIds = orderedIds.ToHashSet();
        if (requestedIds.Count != orderedIds.Count)
        {
            throw new ArgumentException("The reorder request contains duplicate identifiers.", nameof(orderedIds));
        }

        var itemsById = items.ToDictionary(item => item.Id);
        if (orderedIds.Any(id => !itemsById.ContainsKey(id)))
        {
            throw new ArgumentException("The reorder request contains an unknown activity identifier.", nameof(orderedIds));
        }

        var orderedItems = new Queue<T>(orderedIds.Select(id => itemsById[id]));
        for (var index = 0; index < items.Count; index++)
        {
            if (requestedIds.Contains(items[index].Id))
            {
                items[index] = orderedItems.Dequeue();
            }
        }
    }

    private static void EnsureUniqueIds<T>(IEnumerable<T> activities) where T : Activity
    {
        var duplicate = activities.GroupBy(activity => activity.Id)
            .FirstOrDefault(group => group.Key == Guid.Empty || group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidDomainStateException("The data file contains empty or duplicate activity identifiers.");
        }
    }
}
