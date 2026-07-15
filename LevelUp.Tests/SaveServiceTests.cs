using Xunit;
using LevelUp.Domain;
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

        Quest quest = new() { Id = 1 };
        quest.Configure("Quest", "Description");
        quest.AssignToProject(project.Id);
        quest.Activate();
        quest.Complete();

        store.Save(new GameData
        {
            Projects = [project],
            Quests = [quest]
        });

        GameData? loaded = store.Load();

        Assert.NotNull(loaded);
        Assert.Equal("Project", loaded.Projects.Single().Name);
        Assert.Equal(ProjectStatus.Active, loaded.Projects.Single().Status);
        Assert.Equal(project.Id, loaded.Quests.Single().ProjectId);
        Assert.Equal(QuestStatus.Completed, loaded.Quests.Single().Status);
        Assert.NotNull(loaded.Quests.Single().CompletedAt);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
