using LevelUp.Domain;

namespace LevelUp.Services.Persistence;

public sealed class GameDataValidator
{
    public void Validate(GameData gameData)
    {
        ArgumentNullException.ThrowIfNull(gameData);
        EnsureUniqueIds(gameData.Projects.Select(item => item.Id), "projects");
        EnsureUniqueIds(gameData.LegacyQuests.Select(item => item.Id), "tasks");
        EnsureUniqueIds(gameData.Milestones.Select(item => item.Id), "milestones");
        EnsureUniqueIds(gameData.Bosses.Select(item => item.Id), "bosses");
        EnsureUniqueIds(gameData.Books.Select(item => item.Id), "books");
        EnsureUniqueIds(gameData.WalletTags.Select(item => item.Id), "wallet tags");
        EnsureUniqueIds(gameData.WalletTransactions.Select(item => item.Id), "transactions");

        HashSet<int> projectIds = gameData.Projects.Select(item => item.Id).ToHashSet();
        HashSet<int> milestoneIds = gameData.Milestones.Select(item => item.Id).ToHashSet();
        HashSet<int> walletTagIds = gameData.WalletTags.Select(item => item.Id).ToHashSet();

        foreach (var quest in gameData.LegacyQuests)
        {
            if (quest.ProjectId is int projectId && !projectIds.Contains(projectId))
            {
                throw new InvalidDataException($"The task {quest.Id} references a missing project.");
            }
            if (quest.MilestoneId is int milestoneId && !milestoneIds.Contains(milestoneId))
            {
                throw new InvalidDataException($"The task {quest.Id} references a missing milestone.");
            }
        }

        foreach (var milestone in gameData.Milestones)
        {
            if (!projectIds.Contains(milestone.ProjectId))
            {
                throw new InvalidDataException($"The milestone {milestone.Id} does not have a valid project.");
            }
        }

        foreach (var boss in gameData.Bosses)
        {
            if (!projectIds.Contains(boss.ProjectId))
            {
                throw new InvalidDataException($"The boss {boss.Id} does not have a valid project.");
            }
        }


        foreach (var transaction in gameData.WalletTransactions)
        {
            if (transaction.Amount == 0)
            {
                throw new InvalidDataException(
                    $"The transaction {transaction.Id} has a zero value."
                );
            }

            if (transaction.TagId is int tagId && !walletTagIds.Contains(tagId))
            {
                throw new InvalidDataException(
                    $"The transaction {transaction.Id} references a missing tag."
                );
            }
        }

        foreach (var book in gameData.Books)
        {
            if (book.CurrentPage < 0 || book.CurrentPage > book.TotalPages)
            {
                throw new InvalidDataException($"The book progress {book.Id} is invalid.");
            }
        }
    }

    private static void EnsureUniqueIds(IEnumerable<int> ids, string collectionName)
    {
        int[] values = ids.Where(id => id > 0).ToArray();
        if (values.Length != values.Distinct().Count())
        {
            throw new InvalidDataException($"Duplicate IDs were found in {collectionName}.");
        }
    }
}
