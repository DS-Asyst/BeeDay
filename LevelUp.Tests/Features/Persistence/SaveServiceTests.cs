using Xunit;
using LevelUp.Domain;
using LevelUp.Domain.Books;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Wallet;
using LevelUp.Services.Persistence;

namespace LevelUp.Tests;

public sealed class SaveServiceTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        $"LevelUpTests_{Guid.NewGuid():N}"
    );

    [Fact]
    public void SaveAndLoadPreservePrivateSetPropertiesAndRelationships()
    {
        string path = Path.Combine(directory, "save.json");
        SaveService store = new(path);

        Project project = new() { Id = 1 };
        project.Configure("Project", "Description", "Reward");
        project.Activate();

        Milestone milestone = new() { Id = 1 };
        milestone.Configure(project.Id, "Milestone", "Description");

        Quest quest = new() { Id = 1 };
        quest.Configure("Quest", "Description");
        quest.AssignToProject(project.Id);
        quest.AssignToMilestone(milestone.Id, project.Id);
        quest.Activate();
        quest.Complete();

        BossEncounter boss = new() { Id = 1 };
        boss.Configure(project.Id, milestone.Id, "Boss", "Description");

        Book book = new() { Id = 1 };
        book.Configure("Book", "Author", 100);
        book.Start();
        book.RecordProgress(25, new DateTime(2026, 7, 1));

        WalletTransaction transaction = new() { Id = 1 };
        transaction.Configure(
            WalletTransactionType.Deposit,
            500m,
            "Reserve",
            string.Empty,
            new DateTime(2026, 7, 1)
        );

        store.Save(new GameData
        {
            Projects = [project],
            Quests = [quest],
            Milestones = [milestone],
            Bosses = [boss],
            Books = [book],
            WalletTransactions = [transaction]
        });

        GameData? loaded = store.Load();

        Assert.NotNull(loaded);
        Assert.Equal("Project", loaded.Projects.Single().Name);
        Assert.Equal(ProjectStatus.Active, loaded.Projects.Single().Status);
        Assert.Equal(project.Id, loaded.Quests.Single().ProjectId);
        Assert.Equal(QuestStatus.Completed, loaded.Quests.Single().Status);
        Assert.NotNull(loaded.Quests.Single().CompletedAt);
        Assert.Equal(milestone.Id, loaded.Quests.Single().MilestoneId);
        Assert.Equal("Milestone", loaded.Milestones.Single().Title);
        Assert.Equal("Boss", loaded.Bosses.Single().Name);
        Assert.Equal("Book", loaded.Books.Single().Title);
        Assert.Equal(25, loaded.Books.Single().CurrentPage);
        Assert.Single(loaded.Books.Single().ProgressHistory);
        Assert.Equal(500m, loaded.WalletTransactions.Single().Amount);
        Assert.Equal(
            WalletTransactionType.Deposit,
            loaded.WalletTransactions.Single().Type
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
