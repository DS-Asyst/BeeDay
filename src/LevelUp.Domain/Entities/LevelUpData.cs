using System.Text.Json.Serialization;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class LevelUpData
{
    private const string MigrationEmail = "migrated-user@levelup.invalid";

    [JsonInclude]
    public int SchemaVersion { get; private set; } = 3;

    [JsonInclude]
    public Guid? CurrentUserId { get; private set; }

    [JsonInclude]
    public List<User> Users { get; private set; } = [];

    [JsonInclude]
    public List<Character> Characters { get; private set; } = [];

    [JsonInclude, JsonPropertyName("profile")]
    private LegacyProfileSnapshot? LegacyProfile { get; set; }

    [JsonInclude]
    public List<Habit> Habits { get; private set; } = [];

    [JsonInclude]
    public List<RecurringTask> Tasks { get; private set; } = [];

    [JsonInclude]
    public List<Project> Projects { get; private set; } = [];

    [JsonInclude, JsonPropertyName("todos")]
    private List<Todo> LegacyTodos { get; set; } = [];

    [JsonIgnore]
    public User? CurrentUser => CurrentUserId is Guid id ? Users.FirstOrDefault(user => user.Id == id) : null;

    [JsonIgnore]
    public Character? CurrentCharacter => CurrentUserId is Guid id ? Characters.FirstOrDefault(character => character.UserId == id) : null;

    [JsonIgnore]
    public List<Todo> Todos => Projects.SelectMany(project => project.Todos).ToList();

    public void AddUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (Users.Any(existing => string.Equals(existing.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDomainStateException($"Email '{user.Email}' is already registered.");
        }
        Users.Add(user);
        CurrentUserId ??= user.Id;
    }

    public void SetCurrentUser(Guid userId)
    {
        if (Users.All(user => user.Id != userId))
        {
            throw new InvalidDomainStateException($"User '{userId}' was not found.");
        }
        CurrentUserId = userId;
    }

    public void AddCharacter(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);
        if (Users.All(user => user.Id != character.UserId))
        {
            throw new InvalidDomainStateException("Character must belong to an existing User.");
        }

        if (Characters.Any(existing => existing.UserId == character.UserId))
        {
            throw new InvalidDomainStateException("A User can have only one Character.");
        }
        if (Characters.Any(existing => string.Equals(existing.Nickname, character.Nickname, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDomainStateException($"Nickname '@{character.Nickname}' is already in use.");
        }
        Characters.Add(character);
    }

    public void AddHabit(Habit habit) { AssignCurrentOwner(habit); Habits.Add(habit); }
    public void AddTask(RecurringTask task) { AssignCurrentOwner(task); Tasks.Add(task); }
    public void AddProject(Project project) { AssignCurrentOwner(project); Projects.Add(project); }

    public Project FindProject(Guid projectId) => Projects.FirstOrDefault(project => project.Id == projectId)
        ?? throw new InvalidDomainStateException($"Project '{projectId}' was not found.");

    public (Project Project, Todo Todo) FindTodo(Guid todoId)
    {
        foreach (var project in Projects)
        {
            var todo = project.Todos.FirstOrDefault(item => item.Id == todoId);
            if (todo is not null)
            {
                return (project, todo);
            }
        }
        throw new InvalidDomainStateException($"To-Do '{todoId}' was not found.");
    }

    public void AddTodo(Todo todo)
    {
        AssignCurrentOwner(todo);
        FindProject(todo.ProjectId).AddTodo(todo);
    }

    public void ReorderHabits(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Habits, orderedIds);
    public void ReorderTasks(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Tasks, orderedIds);
    public void ReorderProjects(IReadOnlyList<Guid> orderedIds) => ReorderVisibleItems(Projects, orderedIds);

    public void ReorderTodos(IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count < 2)
        {
            return;
        }

        var grouped = orderedIds.Select(FindTodo).GroupBy(x => x.Project.Id);
        foreach (var group in grouped)
        {
            ReorderVisibleItems(group.First().Project.Todos, group.Select(x => x.Todo.Id).ToList());
        }
    }

    public void EnsureValidState()
    {
        SchemaVersion = 3;
        Users ??= [];
        Characters ??= [];
        Habits ??= [];
        Tasks ??= [];
        Projects ??= [];
        LegacyTodos ??= [];

        MigrateLegacyProfile();
        EnsureOwnerForLegacyActivities();

        EnsureUniqueIds(Users);
        EnsureUniqueIds(Characters);
        EnsureUniqueIds(Habits);
        EnsureUniqueIds(Tasks);
        EnsureUniqueIds(Projects);
        EnsureUniqueValues(Users.Select(user => user.Email), "email");
        EnsureUniqueValues(Characters.Select(character => character.Nickname), "nickname");

        if (Users.Count == 0)
        {
            CurrentUserId = null;
        }
        else if (CurrentUserId is null || Users.All(user => user.Id != CurrentUserId))
        {
            CurrentUserId = Users[0].Id;
        }

        var ownerId = CurrentUserId;

        if (LegacyTodos.Count > 0)
        {
            var migrationProject = Projects.FirstOrDefault() ?? Project.Create("Imported To-Dos", "Project created automatically during the Daily domain migration.");
            if (!Projects.Contains(migrationProject))
            {
                Projects.Add(migrationProject);
            }
            foreach (var todo in LegacyTodos)
            {
                if (todo.UserId == Guid.Empty && ownerId is Guid id)
                {
                    todo.AssignOwner(id);
                }

                migrationProject.AddTodo(todo);
            }
            LegacyTodos.Clear();
        }

        foreach (var activity in Habits.Cast<Activity>().Concat(Tasks).Concat(Projects))
        {
            if (activity.UserId == Guid.Empty)
            {
                if (ownerId is not Guid id)
                {
                    throw new InvalidDomainStateException("Activities cannot exist without an owning User.");
                }

                activity.AssignOwner(id);
            }
        }

        foreach (var project in Projects)
        {
            foreach (var todo in project.Todos)
            {
                todo.AssignTo(project.Id);
                if (todo.UserId == Guid.Empty)
                {
                    var todoOwnerId = project.UserId != Guid.Empty ? project.UserId : ownerId;
                    if (todoOwnerId is not Guid id)
                    {
                        throw new InvalidDomainStateException("To-Dos cannot exist without an owning User.");
                    }

                    todo.AssignOwner(id);
                }
            }
        }

        foreach (var character in Characters)
        {
            if (Users.All(user => user.Id != character.UserId))
            {
                throw new InvalidDomainStateException("A Character references an unknown User.");
            }
        }

        if (Characters.GroupBy(character => character.UserId).Any(group => group.Count() > 1))
        {
            throw new InvalidDomainStateException("A User cannot have more than one Character.");
        }

        EnsureUniqueIds(Projects.SelectMany(project => project.Todos));
    }

    private void MigrateLegacyProfile()
    {
        if (LegacyProfile is null)
        {
            return;
        }
        var user = Users.FirstOrDefault() ?? User.Create(LegacyProfile.Name, MigrationEmail);
        if (!Users.Contains(user))
        {
            Users.Add(user);
        }
        else
        {
            user.UpdateName(LegacyProfile.Name);
        }
        CurrentUserId ??= user.Id;
        if (Characters.All(character => character.UserId != user.Id))
        {
            var nickname = string.IsNullOrWhiteSpace(LegacyProfile.Nickname)
                ? LegacyProfile.Name.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant()
                : LegacyProfile.Nickname;
            AddCharacter(Character.Create(user.Id, nickname, LegacyProfile.Class));
        }
        LegacyProfile = null;
    }

    private void EnsureOwnerForLegacyActivities()
    {
        if (Users.Count > 0)
        {
            return;
        }

        var hasLegacyOwnedData = Habits.Count > 0
            || Tasks.Count > 0
            || Projects.Count > 0
            || LegacyTodos.Count > 0;

        if (!hasLegacyOwnedData)
        {
            CurrentUserId = null;
            return;
        }

        var user = User.Create("Migrated User", MigrationEmail);
        Users.Add(user);
        CurrentUserId = user.Id;
    }

    private void AssignCurrentOwner(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        var userId = CurrentUserId
            ?? throw new InvalidDomainStateException("A current User is required before activities can be created.");
        activity.AssignOwner(userId);
    }

    private static void ReorderVisibleItems<T>(List<T> items, IReadOnlyList<Guid> orderedIds) where T : Activity
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

    private static void EnsureUniqueIds<T>(IEnumerable<T> entities) where T : LevelUp.Domain.Abstractions.Entity
    {
        var duplicate = entities.GroupBy(entity => entity.Id).FirstOrDefault(group => group.Key == Guid.Empty || group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidDomainStateException("The data file contains empty or duplicate entity identifiers.");
        }
    }

    private sealed class LegacyProfileSnapshot
    {
        [JsonInclude]
        public string Name { get; private set; } = string.Empty;

        [JsonInclude]
        public string Nickname { get; private set; } = string.Empty;

        [JsonInclude]
        public CharacterClass Class { get; private set; } = CharacterClass.Warrior;
    }

    private static void EnsureUniqueValues(IEnumerable<string> values, string label)
    {
        if (values.GroupBy(value => value, StringComparer.OrdinalIgnoreCase).Any(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
        {
            throw new InvalidDomainStateException($"The data file contains an empty or duplicate {label}.");
        }
    }
}
