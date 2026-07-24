using System.Text.Json.Serialization;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class LevelUpData
{
    [JsonInclude]
    public int SchemaVersion { get; private set; } = 2;

    [JsonInclude]
    public Profile? Profile { get; private set; }

    [JsonInclude]
    public List<Habit> Habits { get; private set; } = [];

    [JsonInclude]
    public List<RecurringTask> Tasks { get; private set; } = [];

    [JsonInclude]
    public List<Project> Projects { get; private set; } = [];

    [JsonInclude, JsonPropertyName("todos")]
    private List<Todo> LegacyTodos { get; set; } = [];

    [JsonIgnore]
    public List<Todo> Todos => Projects.SelectMany(project => project.Todos).ToList();

    public void SetProfile(Profile profile) => Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    public void AddHabit(Habit habit) => Habits.Add(habit ?? throw new ArgumentNullException(nameof(habit)));
    public void AddTask(RecurringTask task) => Tasks.Add(task ?? throw new ArgumentNullException(nameof(task)));
    public void AddProject(Project project) => Projects.Add(project ?? throw new ArgumentNullException(nameof(project)));

    public Project FindProject(Guid projectId) => Projects.FirstOrDefault(project => project.Id == projectId)
        ?? throw new InvalidDomainStateException($"Project '{projectId}' was not found.");

    public (Project Project, Todo Todo) FindTodo(Guid todoId)
    {
        foreach (var project in Projects)
        {
            var todo = project.Todos.FirstOrDefault(item => item.Id == todoId);
            if (todo is not null) return (project, todo);
        }
        throw new InvalidDomainStateException($"To-Do '{todoId}' was not found.");
    }

    public void AddTodo(Todo todo) => FindProject(todo.ProjectId).AddTodo(todo);

    public void ReorderHabits(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Habits, orderedIds);
    public void ReorderTasks(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Tasks, orderedIds);
    public void ReorderProjects(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Projects, orderedIds);

    public void ReorderTodos(IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count < 2) return;
        var grouped = orderedIds.Select(FindTodo).GroupBy(x => x.Project.Id);
        foreach (var group in grouped)
            ReorderVisibleItems(group.First().Project.Todos, group.Select(x => x.Todo.Id).ToList());
    }

    public void EnsureValidState()
    {
        SchemaVersion = 2;
        Habits ??= [];
        Tasks ??= [];
        Projects ??= [];
        LegacyTodos ??= [];

        EnsureUniqueIds(Habits);
        EnsureUniqueIds(Tasks);
        EnsureUniqueIds(Projects);

        if (LegacyTodos.Count > 0)
        {
            var migrationProject = Projects.FirstOrDefault() ?? Project.Create("Imported To-Dos", "Project created automatically during the Daily domain migration.");
            if (!Projects.Contains(migrationProject)) Projects.Add(migrationProject);
            foreach (var todo in LegacyTodos) migrationProject.AddTodo(todo);
            LegacyTodos.Clear();
        }

        foreach (var project in Projects)
        {
            foreach (var todo in project.Todos) todo.AssignTo(project.Id);
        }

        EnsureUniqueIds(Projects.SelectMany(project => project.Todos));
    }

    private static void ReorderVisibleItems<T>(List<T> items, IReadOnlyList<Guid> orderedIds) where T : Activity
    {
        ArgumentNullException.ThrowIfNull(orderedIds);
        if (orderedIds.Count < 2) return;
        var requestedIds = orderedIds.ToHashSet();
        if (requestedIds.Count != orderedIds.Count) throw new ArgumentException("The reorder request contains duplicate identifiers.", nameof(orderedIds));
        var itemsById = items.ToDictionary(item => item.Id);
        if (orderedIds.Any(id => !itemsById.ContainsKey(id))) throw new ArgumentException("The reorder request contains an unknown activity identifier.", nameof(orderedIds));
        var orderedItems = new Queue<T>(orderedIds.Select(id => itemsById[id]));
        for (var index = 0; index < items.Count; index++) if (requestedIds.Contains(items[index].Id)) items[index] = orderedItems.Dequeue();
    }

    private static void EnsureUniqueIds<T>(IEnumerable<T> entities) where T : LevelUp.Domain.Abstractions.Entity
    {
        var duplicate = entities.GroupBy(entity => entity.Id).FirstOrDefault(group => group.Key == Guid.Empty || group.Count() > 1);
        if (duplicate is not null) throw new InvalidDomainStateException("The data file contains empty or duplicate entity identifiers.");
    }
}
