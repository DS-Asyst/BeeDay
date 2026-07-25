using System.Text.Json.Serialization;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed class LevelUpData
{
    private const string MigrationEmail = "migrated-user@levelup.invalid";

    [JsonInclude]
    public int SchemaVersion { get; private set; } = 4;

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

    [JsonInclude]
    public List<Wallet> Wallets { get; private set; } = [];

    [JsonInclude]
    public List<Transaction> Transactions { get; private set; } = [];

    [JsonInclude]
    public List<InventoryTag> InventoryTags { get; private set; } = [];

    [JsonInclude, JsonPropertyName("todos")]
    private List<Todo> LegacyTodos { get; set; } = [];

    [JsonIgnore]
    public User? CurrentUser => CurrentUserId is Guid id ? Users.FirstOrDefault(user => user.Id == id) : null;

    [JsonIgnore]
    public Character? CurrentCharacter => CurrentUserId is Guid id ? Characters.FirstOrDefault(character => character.UserId == id) : null;

    [JsonIgnore]
    public List<Todo> Todos => Projects.SelectMany(project => project.Todos).ToList();

    public User FindUser(Guid userId) => Users.FirstOrDefault(user => user.Id == userId)
        ?? throw new InvalidDomainStateException($"User '{userId}' was not found.");

    public Character? FindCharacterForUser(Guid userId) =>
        Characters.FirstOrDefault(character => character.UserId == userId);

    public LevelUpData CreateUserSnapshot(Guid userId)
    {
        var user = FindUser(userId);
        var projects = Projects.Where(project => project.UserId == userId).ToList();
        return new LevelUpData
        {
            SchemaVersion = SchemaVersion,
            CurrentUserId = userId,
            Users = [user],
            Characters = Characters.Where(character => character.UserId == userId).ToList(),
            Habits = Habits.Where(habit => habit.UserId == userId).ToList(),
            Tasks = Tasks.Where(task => task.UserId == userId).ToList(),
            Projects = projects,
            Wallets = Wallets.Where(wallet => wallet.UserId == userId).ToList(),
            InventoryTags = InventoryTags.Where(tag => tag.UserId == userId).ToList(),
            Transactions = Transactions.Where(transaction =>
                Wallets.Any(wallet => wallet.Id == transaction.WalletId && wallet.UserId == userId)).ToList()
        };
    }

    public void AddHabit(Guid userId, Habit habit) { AssignOwner(userId, habit); Habits.Add(habit); }
    public void AddTask(Guid userId, RecurringTask task) { AssignOwner(userId, task); Tasks.Add(task); }
    public void AddProject(Guid userId, Project project) { AssignOwner(userId, project); Projects.Add(project); }

    public Habit FindHabit(Guid userId, Guid habitId) => Habits.FirstOrDefault(item => item.Id == habitId && item.UserId == userId)
        ?? throw new InvalidDomainStateException($"Habit '{habitId}' was not found for the authenticated User.");

    public RecurringTask FindTask(Guid userId, Guid taskId) => Tasks.FirstOrDefault(item => item.Id == taskId && item.UserId == userId)
        ?? throw new InvalidDomainStateException($"Task '{taskId}' was not found for the authenticated User.");

    public Project FindProject(Guid userId, Guid projectId) => Projects.FirstOrDefault(project => project.Id == projectId && project.UserId == userId)
        ?? throw new InvalidDomainStateException($"Project '{projectId}' was not found for the authenticated User.");

    public (Project Project, Todo Todo) FindTodo(Guid userId, Guid todoId)
    {
        foreach (var project in Projects.Where(project => project.UserId == userId))
        {
            var todo = project.Todos.FirstOrDefault(item => item.Id == todoId && item.UserId == userId);
            if (todo is not null)
            {
                return (project, todo);
            }
        }
        throw new InvalidDomainStateException($"To-Do '{todoId}' was not found for the authenticated User.");
    }

    public void ReorderHabits(Guid userId, IReadOnlyList<Guid> ids) => ReorderOwnedItems(Habits, userId, ids);
    public void ReorderTasks(Guid userId, IReadOnlyList<Guid> ids) => ReorderOwnedItems(Tasks, userId, ids);
    public void ReorderProjects(Guid userId, IReadOnlyList<Guid> ids) => ReorderOwnedItems(Projects, userId, ids);

    public void ReorderTodos(Guid userId, IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count < 2)
        {
            return;
        }
        var grouped = orderedIds.Select(id => FindTodo(userId, id)).GroupBy(x => x.Project.Id);
        foreach (var group in grouped)
        {
            ReorderVisibleItems(group.First().Project.Todos, group.Select(x => x.Todo.Id).ToList());
        }
    }

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

    public void AddWallet(Wallet wallet)
    {
        ArgumentNullException.ThrowIfNull(wallet);
        if (Users.All(user => user.Id != wallet.UserId))
        {
            throw new InvalidDomainStateException("Wallet must belong to an existing User.");
        }
        if (Wallets.Any(existing => existing.UserId == wallet.UserId))
        {
            throw new InvalidDomainStateException("A User can have only one Wallet.");
        }
        Wallets.Add(wallet);
    }

    public void AddInventoryTag(InventoryTag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (Users.All(user => user.Id != tag.UserId))
        {
            throw new InvalidDomainStateException("Inventory tag must belong to an existing User.");
        }
        if (InventoryTags.Any(existing => existing.UserId == tag.UserId
            && string.Equals(existing.Name, tag.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDomainStateException($"Inventory tag '{tag.Name}' already exists for this User.");
        }
        InventoryTags.Add(tag);
    }

    public void AddTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        var wallet = FindWallet(transaction.WalletId);
        ValidateTransactionTagOwnership(transaction, wallet);
        Transactions.Add(transaction);
        wallet.Touch();
    }

    public Wallet FindWallet(Guid walletId) => Wallets.FirstOrDefault(wallet => wallet.Id == walletId)
        ?? throw new InvalidDomainStateException($"Wallet '{walletId}' was not found.");

    public InventoryTag FindInventoryTag(Guid tagId) => InventoryTags.FirstOrDefault(tag => tag.Id == tagId)
        ?? throw new InvalidDomainStateException($"Inventory tag '{tagId}' was not found.");

    public Transaction FindTransaction(Guid transactionId) => Transactions.FirstOrDefault(transaction => transaction.Id == transactionId)
        ?? throw new InvalidDomainStateException($"Transaction '{transactionId}' was not found.");

    public void RemoveInventoryTag(Guid tagId)
    {
        var tag = FindInventoryTag(tagId);
        foreach (var transaction in Transactions.Where(transaction => transaction.InventoryTagId == tagId))
        {
            transaction.RemoveTag();
        }
        InventoryTags.Remove(tag);
    }

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
        SchemaVersion = 4;
        Users ??= [];
        Characters ??= [];
        Habits ??= [];
        Tasks ??= [];
        Projects ??= [];
        Wallets ??= [];
        Transactions ??= [];
        InventoryTags ??= [];
        LegacyTodos ??= [];

        MigrateLegacyProfile();
        EnsureOwnerForLegacyActivities();

        EnsureUniqueIds(Users);
        EnsureUniqueIds(Characters);
        EnsureUniqueIds(Habits);
        EnsureUniqueIds(Tasks);
        EnsureUniqueIds(Projects);
        EnsureUniqueIds(Wallets);
        EnsureUniqueIds(Transactions);
        EnsureUniqueIds(InventoryTags);
        EnsureUniqueValues(Users.Select(user => user.Email), "email");
        EnsureUniqueValues(Characters.Select(character => character.Nickname), "nickname");
        EnsureUniqueInventoryTagNames();

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

        foreach (var wallet in Wallets)
        {
            if (Users.All(user => user.Id != wallet.UserId))
            {
                throw new InvalidDomainStateException("A Wallet references an unknown User.");
            }
        }

        if (Wallets.GroupBy(wallet => wallet.UserId).Any(group => group.Count() > 1))
        {
            throw new InvalidDomainStateException("A User cannot have more than one Wallet.");
        }

        foreach (var tag in InventoryTags)
        {
            if (Users.All(user => user.Id != tag.UserId))
            {
                throw new InvalidDomainStateException("An Inventory tag references an unknown User.");
            }
        }

        foreach (var transaction in Transactions)
        {
            var wallet = FindWallet(transaction.WalletId);
            ValidateTransactionTagOwnership(transaction, wallet);
        }
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


    private void AssignOwner(Guid userId, Activity activity)
    {
        FindUser(userId);
        activity.AssignOwner(userId);
    }

    private static void ReorderOwnedItems<T>(List<T> allItems, Guid userId, IReadOnlyList<Guid> orderedIds) where T : Activity
    {
        var ownedItems = allItems.Where(item => item.UserId == userId).ToList();
        if (orderedIds.Any(id => ownedItems.All(item => item.Id != id)))
        {
            throw new InvalidDomainStateException("The reorder request contains an activity owned by another User.");
        }

        ReorderVisibleItems(ownedItems, orderedIds);
        var reordered = new Queue<T>(ownedItems);
        for (var index = 0; index < allItems.Count; index++)
        {
            if (allItems[index].UserId == userId)
            {
                allItems[index] = reordered.Dequeue();
            }
        }
    }

    private void ValidateTransactionTagOwnership(Transaction transaction, Wallet wallet)
    {
        if (transaction.InventoryTagId is not Guid tagId)
        {
            return;
        }

        var tag = FindInventoryTag(tagId);
        if (tag.UserId != wallet.UserId)
        {
            throw new InvalidDomainStateException("A Transaction cannot use an Inventory tag owned by another User.");
        }
    }

    private void EnsureUniqueInventoryTagNames()
    {
        if (InventoryTags
            .GroupBy(tag => new { tag.UserId, Name = tag.Name.ToUpperInvariant() })
            .Any(group => string.IsNullOrWhiteSpace(group.Key.Name) || group.Count() > 1))
        {
            throw new InvalidDomainStateException("The data file contains an empty or duplicate Inventory tag name for the same User.");
        }
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
