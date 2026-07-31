using System.Text.Json.Serialization;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Entities;

public sealed partial class LevelUpData
{
    public void EnsureValidState()
    {
        var sourceSchemaVersion = SchemaVersion;
        SchemaVersion = 7;
        Users ??= [];
        UserTokens ??= [];
        Habits ??= [];
        Tasks ??= [];
        Projects ??= [];
        Wallets ??= [];
        Transactions ??= [];
        WalletTags ??= [];
        LegacyTodos ??= [];

        MigrateLegacyProfile();
        EnsureOwnerForLegacyActivities();
        ConfirmEmailsForExistingUsers(sourceSchemaVersion);

        EnsureUniqueIds(Users);
        EnsureUniqueIds(UserTokens);
        EnsureUniqueIds(Habits);
        EnsureUniqueIds(Tasks);
        EnsureUniqueIds(Projects);
        EnsureUniqueIds(Wallets);
        EnsureUniqueIds(Transactions);
        EnsureUniqueIds(WalletTags);
        EnsureUniqueValues(Users.Select(user => user.Email), "email");
        EnsureUniqueNicknames();
        EnsureUniqueWalletTagNames();

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

        foreach (var token in UserTokens)
        {
            if (Users.All(user => user.Id != token.UserId))
            {
                throw new InvalidDomainStateException("A User token references an unknown User.");
            }

            if (string.IsNullOrWhiteSpace(token.TokenHash))
            {
                throw new InvalidDomainStateException("A User token cannot have an empty hash.");
            }

            if (token.ExpiresAtUtc <= token.CreatedAtUtc)
            {
                throw new InvalidDomainStateException("A User token has an invalid expiration time.");
            }
        }

        foreach (var user in Users)
        {
            user.EnsureExperienceState();
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

        foreach (var tag in WalletTags)
        {
            if (Users.All(user => user.Id != tag.UserId))
            {
                throw new InvalidDomainStateException("A Wallet tag references an unknown User.");
            }
        }

        foreach (var transaction in Transactions)
        {
            var wallet = FindWallet(transaction.WalletId);
            ValidateTransactionTagOwnership(transaction, wallet);
        }
    }

    private void ConfirmEmailsForExistingUsers(int sourceSchemaVersion)
    {
        if (sourceSchemaVersion >= 5)
        {
            return;
        }

        foreach (var user in Users.Where(user => !user.IsEmailConfirmed))
        {
            user.ConfirmEmail(user.CreatedAtUtc);
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
        if (!user.HasProfile)
        {
            var nickname = string.IsNullOrWhiteSpace(LegacyProfile.Nickname)
                ? LegacyProfile.Name.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant()
                : LegacyProfile.Nickname;
            CompleteUserProfile(user.Id, nickname);
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
        if (transaction.WalletTagId is not Guid tagId)
        {
            return;
        }

        var tag = FindWalletTag(tagId);
        if (tag.UserId != wallet.UserId)
        {
            throw new InvalidDomainStateException("A Transaction cannot use a Wallet tag owned by another User.");
        }
    }

    private void EnsureUniqueNicknames()
    {
        if (Users
            .Where(user => !string.IsNullOrWhiteSpace(user.Nickname))
            .GroupBy(user => user.Nickname, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidDomainStateException("The data file contains a duplicate nickname for a different User.");
        }
    }

    private void EnsureUniqueWalletTagNames()
    {
        if (WalletTags
            .GroupBy(tag => new { tag.UserId, Name = tag.Name.ToUpperInvariant() })
            .Any(group => string.IsNullOrWhiteSpace(group.Key.Name) || group.Count() > 1))
        {
            throw new InvalidDomainStateException("The data file contains an empty or duplicate Wallet tag name for the same User.");
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
    }

    private static void EnsureUniqueValues(IEnumerable<string> values, string label)
    {
        if (values.GroupBy(value => value, StringComparer.OrdinalIgnoreCase).Any(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
        {
            throw new InvalidDomainStateException($"The data file contains an empty or duplicate {label}.");
        }
    }
}
