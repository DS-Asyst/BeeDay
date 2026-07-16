using Xunit;
using LevelUp.Domain;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
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

        store.Save(new GameData
        {
            Projects = [project],
            Quests = [quest],
            Milestones = [milestone],
            Bosses = [boss]
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
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
