using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Tests;

/// <summary>
/// Shared in-memory double for the 8 per-Aggregate persistence contracts plus <see cref="IUnitOfWork"/>
/// (docs/architecture/07-persistence-contracts.md), replacing the Sprint 13.4-era
/// <c>FakeLevelUpRepository</c> now that no production handler depends on <c>ILevelUpRepository</c>
/// (Sprint 14.6). Backed by 8 independent <c>List&lt;T&gt;</c> collections, one per Aggregate — no type
/// aggregates them together (Sprint 14.7 removed <c>LevelUpData</c>, the last such type, precisely to
/// stop this kind of fake from having one to reuse). Each fake repository only touches its own list,
/// mirroring how each real <c>EfXxxRepository</c> only touches its own rows with no cross-Aggregate
/// checks. <c>UpdateAsync</c> methods use <c>.Single(...)</c> (not a "not found"-friendly lookup) to
/// faithfully reproduce the real adapters, which throw a generic exception on a missing row — exactly
/// what the handlers' own explicit existence checks exist to guard against.
/// </summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public List<User> UsersData { get; } = [];
    public List<UserToken> UserTokensData { get; } = [];
    public List<Habit> HabitsData { get; } = [];
    public List<RecurringTask> RecurringTasksData { get; } = [];
    public List<Project> ProjectsData { get; } = [];
    public List<Wallet> WalletsData { get; } = [];
    public List<WalletTag> WalletTagsData { get; } = [];
    public List<Transaction> TransactionsData { get; } = [];

    public IUserRepository Users { get; }
    public IUserTokenRepository UserTokens { get; }
    public IHabitRepository Habits { get; }
    public IRecurringTaskRepository RecurringTasks { get; }
    public IProjectRepository Projects { get; }
    public IWalletRepository Wallets { get; }
    public IWalletTagRepository WalletTags { get; }
    public ITransactionRepository Transactions { get; }

    public FakeUnitOfWork()
    {
        Users = new FakeUserRepository(UsersData);
        UserTokens = new FakeUserTokenRepository(UserTokensData);
        Habits = new FakeHabitRepository(HabitsData);
        RecurringTasks = new FakeRecurringTaskRepository(RecurringTasksData);
        Projects = new FakeProjectRepository(ProjectsData);
        Wallets = new FakeWalletRepository(WalletsData);
        WalletTags = new FakeWalletTagRepository(WalletTagsData);
        Transactions = new FakeTransactionRepository(TransactionsData);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed class FakeUserRepository(List<User> data) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(user => user.Id == userId));

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(user => string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> IsEmailInUseAsync(string normalizedEmail, Guid? excludingUserId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.Any(user =>
            string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) && user.Id != excludingUserId));

    public Task<bool> IsNicknameInUseAsync(string normalizedNickname, Guid? excludingUserId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.Any(user =>
            string.Equals(user.Nickname, normalizedNickname, StringComparison.OrdinalIgnoreCase) && user.Id != excludingUserId));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        data.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid userId, Action<User> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(user => user.Id == userId));
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserTokenRepository(List<UserToken> data) : IUserTokenRepository
{
    public Task<UserToken?> GetByHashAsync(string tokenHash, UserTokenType type, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(token => token.TokenHash == tokenHash && token.Type == type));

    public Task<IReadOnlyList<UserToken>> ListActiveAsync(Guid userId, UserTokenType type, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        IReadOnlyList<UserToken> tokens = data
            .Where(token => token.UserId == userId && token.Type == type &&
                token.UsedAtUtc is null && token.RevokedAtUtc is null && token.ExpiresAtUtc > now)
            .ToList();
        return Task.FromResult(tokens);
    }

    public Task AddAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        data.Add(token);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid tokenId, Action<UserToken> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(token => token.Id == tokenId));
        return Task.CompletedTask;
    }

    public Task RevokeActiveAsync(Guid userId, UserTokenType type, DateTimeOffset revokedAtUtc, CancellationToken cancellationToken = default)
    {
        foreach (var token in data.Where(token => token.UserId == userId && token.Type == type &&
            token.UsedAtUtc is null && token.RevokedAtUtc is null))
        {
            token.Revoke(revokedAtUtc);
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeHabitRepository(List<Habit> data) : IHabitRepository
{
    public Task<Habit?> GetAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(habit => habit.Id == habitId && habit.UserId == userId));

    public Task<IReadOnlyList<Habit>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Habit> habits = data.Where(habit => habit.UserId == userId).ToList();
        return Task.FromResult(habits);
    }

    public Task AddAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        data.Add(habit);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        data.RemoveAll(existing => existing.Id == habit.Id);
        return Task.CompletedTask;
    }

    public Task ReorderAsync(Guid userId, IReadOnlyList<Guid> orderedHabitIds, CancellationToken cancellationToken = default)
    {
        Reorder(data, userId, orderedHabitIds);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid userId, Guid habitId, Action<Habit> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(habit => habit.Id == habitId && habit.UserId == userId));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Mirrors LevelUpData.ReorderVisibleItems (removed, Sprint 14.7): fills only the slots already
    /// occupied by <paramref name="orderedIds"/> with those same items in the requested order — Domain
    /// has no Position property (that is Infrastructure-only, an EF shadow property), so the fake
    /// represents order via list-slot position instead, exactly like the JSON-era implementation did.
    /// </summary>
    internal static void Reorder<T>(List<T> all, Guid userId, IReadOnlyList<Guid> orderedIds) where T : Activity
    {
        if (orderedIds.Count < 2)
        {
            return;
        }

        var owned = all.Where(item => item.UserId == userId).ToDictionary(item => item.Id);
        if (orderedIds.Any(id => !owned.ContainsKey(id)))
        {
            return;
        }

        var requestedIds = orderedIds.ToHashSet();
        var queue = new Queue<T>(orderedIds.Select(id => owned[id]));
        for (var index = 0; index < all.Count; index++)
        {
            if (requestedIds.Contains(all[index].Id))
            {
                all[index] = queue.Dequeue();
            }
        }
    }
}

internal sealed class FakeRecurringTaskRepository(List<RecurringTask> data) : IRecurringTaskRepository
{
    public Task<RecurringTask?> GetAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(task => task.Id == taskId && task.UserId == userId));

    public Task<IReadOnlyList<RecurringTask>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RecurringTask> tasks = data.Where(task => task.UserId == userId).ToList();
        return Task.FromResult(tasks);
    }

    public Task AddAsync(RecurringTask task, CancellationToken cancellationToken = default)
    {
        data.Add(task);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(RecurringTask task, CancellationToken cancellationToken = default)
    {
        data.RemoveAll(existing => existing.Id == task.Id);
        return Task.CompletedTask;
    }

    public Task ReorderAsync(Guid userId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default)
    {
        FakeHabitRepository.Reorder(data, userId, orderedTaskIds);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid userId, Guid taskId, Action<RecurringTask> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(task => task.Id == taskId && task.UserId == userId));
        return Task.CompletedTask;
    }
}

internal sealed class FakeProjectRepository(List<Project> data) : IProjectRepository
{
    public Task<Project?> GetAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(project => project.Id == projectId && project.UserId == userId));

    public Task<IReadOnlyList<Project>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Project> projects = data.Where(project => project.UserId == userId).ToList();
        return Task.FromResult(projects);
    }

    public Task<Project?> GetByTodoIdAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(project =>
            project.UserId == userId && project.Todos.Any(todo => todo.Id == todoId)));

    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        data.Add(project);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Project project, CancellationToken cancellationToken = default)
    {
        data.RemoveAll(existing => existing.Id == project.Id);
        return Task.CompletedTask;
    }

    public Task ReorderAsync(Guid userId, IReadOnlyList<Guid> orderedProjectIds, CancellationToken cancellationToken = default)
    {
        FakeHabitRepository.Reorder(data, userId, orderedProjectIds);
        return Task.CompletedTask;
    }

    public Task ReorderTodosAsync(Guid userId, Guid projectId, IReadOnlyList<Guid> orderedTodoIds, CancellationToken cancellationToken = default)
    {
        if (orderedTodoIds.Count < 2)
        {
            return Task.CompletedTask;
        }

        var project = data.Single(candidate => candidate.Id == projectId && candidate.UserId == userId);
        var todos = project.Todos;
        var byId = todos.Where(todo => orderedTodoIds.Contains(todo.Id)).ToDictionary(todo => todo.Id);
        if (orderedTodoIds.Any(id => !byId.ContainsKey(id)))
        {
            return Task.CompletedTask;
        }

        var requestedIds = orderedTodoIds.ToHashSet();
        var queue = new Queue<Todo>(orderedTodoIds.Select(id => byId[id]));
        for (var index = 0; index < todos.Count; index++)
        {
            if (requestedIds.Contains(todos[index].Id))
            {
                todos[index] = queue.Dequeue();
            }
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid userId, Guid projectId, Action<Project> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(project => project.Id == projectId && project.UserId == userId));
        return Task.CompletedTask;
    }

    public Task AddTodoAsync(Guid userId, Guid projectId, Todo todo, CancellationToken cancellationToken = default)
    {
        var project = data.Single(candidate => candidate.Id == projectId && candidate.UserId == userId);
        project.AddTodo(todo);
        return Task.CompletedTask;
    }

    public Task UpdateTodoAsync(Guid userId, Guid todoId, Action<Todo> mutation, CancellationToken cancellationToken = default)
    {
        var project = data.Single(candidate => candidate.UserId == userId && candidate.Todos.Any(todo => todo.Id == todoId));
        mutation(project.Todos.Single(todo => todo.Id == todoId));
        return Task.CompletedTask;
    }

    public Task RemoveTodoAsync(Guid userId, Guid todoId, CancellationToken cancellationToken = default)
    {
        var project = data.Single(candidate => candidate.UserId == userId && candidate.Todos.Any(todo => todo.Id == todoId));
        project.RemoveTodo(todoId);
        return Task.CompletedTask;
    }

    public Task MoveTodoAsync(Guid userId, Guid todoId, Guid destinationProjectId, CancellationToken cancellationToken = default)
    {
        var source = data.Single(candidate => candidate.UserId == userId && candidate.Todos.Any(todo => todo.Id == todoId));
        var destination = data.Single(candidate => candidate.Id == destinationProjectId && candidate.UserId == userId);
        var todo = source.Todos.Single(candidate => candidate.Id == todoId);
        source.RemoveTodo(todoId);
        destination.AddTodo(todo);
        return Task.CompletedTask;
    }
}

internal sealed class FakeWalletRepository(List<Wallet> data) : IWalletRepository
{
    public Task<Wallet?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(wallet => wallet.UserId == userId));

    public Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        data.Add(wallet);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid userId, Action<Wallet> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(wallet => wallet.UserId == userId));
        return Task.CompletedTask;
    }
}

internal sealed class FakeWalletTagRepository(List<WalletTag> data) : IWalletTagRepository
{
    public Task<WalletTag?> GetAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(tag => tag.Id == tagId && tag.UserId == userId));

    public Task<IReadOnlyList<WalletTag>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WalletTag> tags = data.Where(tag => tag.UserId == userId).ToList();
        return Task.FromResult(tags);
    }

    public Task<bool> IsNameInUseAsync(Guid userId, string normalizedName, Guid? excludingTagId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.Any(tag =>
            tag.UserId == userId && tag.Id != excludingTagId && string.Equals(tag.Name, normalizedName, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(WalletTag tag, CancellationToken cancellationToken = default)
    {
        data.Add(tag);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(WalletTag tag, CancellationToken cancellationToken = default)
    {
        data.RemoveAll(existing => existing.Id == tag.Id);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid userId, Guid tagId, Action<WalletTag> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(tag => tag.Id == tagId && tag.UserId == userId));
        return Task.CompletedTask;
    }
}

internal sealed class FakeTransactionRepository(List<Transaction> data) : ITransactionRepository
{
    public Task<Transaction?> GetAsync(Guid walletId, Guid transactionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(data.FirstOrDefault(transaction => transaction.Id == transactionId && transaction.WalletId == walletId));

    public Task<IReadOnlyList<Transaction>> ListByTagAsync(Guid walletTagId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Transaction> transactions = data.Where(transaction => transaction.WalletTagId == walletTagId).ToList();
        return Task.FromResult(transactions);
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        data.Add(transaction);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        data.RemoveAll(existing => existing.Id == transaction.Id);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid walletId, Guid transactionId, Action<Transaction> mutation, CancellationToken cancellationToken = default)
    {
        mutation(data.Single(transaction => transaction.Id == transactionId && transaction.WalletId == walletId));
        return Task.CompletedTask;
    }

    public Task ClearTagReferencesAsync(Guid walletTagId, CancellationToken cancellationToken = default)
    {
        foreach (var transaction in data.Where(transaction => transaction.WalletTagId == walletTagId))
        {
            transaction.RemoveTag();
        }

        return Task.CompletedTask;
    }
}
