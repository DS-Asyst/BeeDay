using Xunit;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;

namespace LevelUp.Tests;

public sealed class ProjectAndQuestTests
{
    [Fact]
    public void ActiveProjectCompletesWhenAllNonArchivedQuestsAreCompleted()
    {
        ProjectService projects = new();
        QuestService quests = new();
        Project project = projects.CreateProject("Project", "Description", "Title");
        projects.ActivateProject(project);

        Quest first = quests.CreateQuest("First", "Description", project);
        Quest second = quests.CreateQuest("Second", "Description", project);
        quests.ActivateQuest(first);
        quests.ActivateQuest(second);
        quests.CompleteQuest(first);
        quests.CompleteQuest(second);

        bool completed = projects.TryCompleteProject(project, quests.GetAllQuests());

        Assert.True(completed);
        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.Equal(100m, projects.CalculateProgress(project, quests.GetAllQuests()));
    }

    [Fact]
    public void ArchivedQuestsDoNotAffectProgress()
    {
        ProjectService projects = new();
        QuestService quests = new();
        Project project = projects.CreateProject("Project", "Description", "Title");

        Quest completed = quests.CreateQuest("Completed", "Description", project);
        Quest archived = quests.CreateQuest("Archived", "Description", project);
        quests.ActivateQuest(completed);
        quests.CompleteQuest(completed);
        quests.ArchiveQuest(archived);

        decimal progress = projects.CalculateProgress(project, quests.GetAllQuests());

        Assert.Equal(100m, progress);
    }

    [Fact]
    public void ArchivedQuestCannotBeEditedOrReassigned()
    {
        Quest quest = new() { Id = 1 };
        quest.Configure("Quest", "Description");
        quest.Archive();

        Assert.Throws<InvalidOperationException>(
            () => quest.UpdateDetails("Changed", "Changed")
        );
        Assert.Throws<InvalidOperationException>(
            () => quest.AssignToProject(1)
        );
    }
}
