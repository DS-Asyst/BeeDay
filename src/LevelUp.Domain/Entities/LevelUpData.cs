using System.Text.Json.Serialization;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed partial class LevelUpData
{
    private const string MigrationEmail = "migrated-user@levelup.invalid";

    [JsonInclude]
    public int SchemaVersion { get; private set; } = 5;

    [JsonInclude]
    public Guid? CurrentUserId { get; private set; }

    [JsonInclude]
    public List<User> Users { get; private set; } = [];

    [JsonInclude]
    public List<UserToken> UserTokens { get; private set; } = [];

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
            UserTokens = UserTokens.Where(token => token.UserId == userId).ToList(),
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

    public void AddUserToken(UserToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        FindUser(token.UserId);

        if (UserTokens.Any(existing => existing.Id == token.Id))
        {
            throw new InvalidDomainStateException($"User token '{token.Id}' is already registered.");
        }

        UserTokens.Add(token);
    }

    public UserToken FindUserToken(Guid tokenId) => UserTokens.FirstOrDefault(token => token.Id == tokenId)
        ?? throw new InvalidDomainStateException($"User token '{tokenId}' was not found.");

    public void RevokeActiveUserTokens(Guid userId, UserTokenType type, DateTimeOffset revokedAtUtc)
    {
        FindUser(userId);
        foreach (var token in UserTokens.Where(token => token.UserId == userId && token.Type == type))
        {
            token.Revoke(revokedAtUtc);
        }
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
}
