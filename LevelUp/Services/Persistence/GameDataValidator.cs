using LevelUp.Domain;

namespace LevelUp.Services.Persistence;

public sealed class GameDataValidator
{
    public void Validate(GameData gameData)
    {
        ArgumentNullException.ThrowIfNull(gameData);
        EnsureUniqueIds(gameData.Projects.Select(item => item.Id), "projetos");
        EnsureUniqueIds(gameData.Quests.Select(item => item.Id), "missões");
        EnsureUniqueIds(gameData.Milestones.Select(item => item.Id), "capítulos");
        EnsureUniqueIds(gameData.Bosses.Select(item => item.Id), "chefes");
        EnsureUniqueIds(gameData.Books.Select(item => item.Id), "livros");
        EnsureUniqueIds(gameData.WalletTransactions.Select(item => item.Id), "movimentações");
        EnsureUniqueIds(gameData.Goals.Select(item => item.Id), "metas");

        HashSet<int> projectIds = gameData.Projects.Select(item => item.Id).ToHashSet();
        HashSet<int> milestoneIds = gameData.Milestones.Select(item => item.Id).ToHashSet();

        foreach (var quest in gameData.Quests)
        {
            if (quest.ProjectId is int projectId && !projectIds.Contains(projectId))
            {
                throw new InvalidDataException($"A missão {quest.Id} aponta para um projeto inexistente.");
            }
            if (quest.MilestoneId is int milestoneId && !milestoneIds.Contains(milestoneId))
            {
                throw new InvalidDataException($"A missão {quest.Id} aponta para um capítulo inexistente.");
            }
        }

        foreach (var milestone in gameData.Milestones)
        {
            if (!projectIds.Contains(milestone.ProjectId))
            {
                throw new InvalidDataException($"O capítulo {milestone.Id} não possui um projeto válido.");
            }
        }

        foreach (var boss in gameData.Bosses)
        {
            if (!projectIds.Contains(boss.ProjectId))
            {
                throw new InvalidDataException($"O chefe {boss.Id} não possui um projeto válido.");
            }
        }

        foreach (var book in gameData.Books)
        {
            if (book.CurrentPage < 0 || book.CurrentPage > book.TotalPages)
            {
                throw new InvalidDataException($"O progresso do livro {book.Id} é inválido.");
            }
        }
    }

    private static void EnsureUniqueIds(IEnumerable<int> ids, string collectionName)
    {
        int[] values = ids.Where(id => id > 0).ToArray();
        if (values.Length != values.Distinct().Count())
        {
            throw new InvalidDataException($"Existem IDs duplicados em {collectionName}.");
        }
    }
}
