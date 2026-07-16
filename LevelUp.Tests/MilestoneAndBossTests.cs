using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using Xunit;

namespace LevelUp.Tests;

public sealed class MilestoneAndBossTests
{
    [Fact]
    public void ProjectCanHaveOnlyOneActiveMilestone()
    {
        ProjectService projects = new();
        MilestoneService milestones = new();
        var project = projects.CreateProject("Project", "Description", "Reward");
        projects.ActivateProject(project);

        var first = milestones.CreateMilestone(project, "First", "Description", 1);
        var second = milestones.CreateMilestone(project, "Second", "Description", 2);
        milestones.Activate(first);

        Assert.Throws<InvalidOperationException>(() => milestones.Activate(second));
    }

    [Fact]
    public void CompletingMilestoneUnlocksAndActivatesNext()
    {
        ProjectService projects = new();
        QuestService quests = new();
        MilestoneService milestones = new();
        var project = projects.CreateProject("Project", "Description", "Reward");
        var first = milestones.CreateMilestone(project, "First", "Description", 1);
        var second = milestones.CreateMilestone(project, "Second", "Description", 2);
        milestones.Activate(first);

        var quest = quests.CreateQuest("Quest", "Description", project);
        quests.AssignQuestToMilestone(quest, first);
        quests.ActivateQuest(quest);
        quests.CompleteQuest(quest);

        Assert.True(milestones.TryComplete(first, quests.GetAllQuests()));
        Assert.Equal(MilestoneStatus.Active, milestones.UnlockAndActivateNext(first)!.Status);
        Assert.Equal(MilestoneStatus.Completed, first.Status);
        Assert.Equal(MilestoneStatus.Active, second.Status);
    }

    [Fact]
    public void QuestMilestoneMustBelongToSameProject()
    {
        ProjectService projects = new();
        QuestService quests = new();
        MilestoneService milestones = new();
        var firstProject = projects.CreateProject("First", "Description", "Reward");
        var secondProject = projects.CreateProject("Second", "Description", "Reward");
        var milestone = milestones.CreateMilestone(secondProject, "Milestone", "Description", 1);
        var quest = quests.CreateQuest("Quest", "Description", firstProject);

        Assert.Throws<InvalidOperationException>(
            () => quests.AssignQuestToMilestone(quest, milestone)
        );
    }

    [Fact]
    public void CompletedMilestoneCannotReceiveNewQuest()
    {
        ProjectService projects = new();
        QuestService quests = new();
        MilestoneService milestones = new();
        var project = projects.CreateProject("Project", "Description", "Reward");
        var milestone = milestones.CreateMilestone(project, "Milestone", "Description", 1);
        milestones.Activate(milestone);
        milestones.CompleteManually(milestone, quests.GetAllQuests());
        var quest = quests.CreateQuest("Quest", "Description", project);

        Assert.Throws<InvalidOperationException>(
            () => quests.AssignQuestToMilestone(quest, milestone)
        );
    }

    [Fact]
    public void BossUnlocksAfterMilestoneCompletion()
    {
        ProjectService projects = new();
        QuestService quests = new();
        MilestoneService milestones = new();
        BossService bosses = new();
        var project = projects.CreateProject("Project", "Description", "Reward");
        var milestone = milestones.CreateMilestone(project, "Milestone", "Description", 1);
        var boss = bosses.Create(project, milestone, "Boss", "Description");
        milestones.Activate(milestone);

        Assert.True(bosses.TryUnlockForMilestoneRequirement(milestone, requirementsMet: true));
        Assert.Equal(BossStatus.Available, boss.Status);
    }

    [Fact]
    public void RewardCanBeClaimedOnlyOnceAfterCompletion()
    {
        Milestone milestone = new();
        milestone.Configure(
            1,
            "Milestone",
            "Description",
            reward: new MilestoneReward(Experience: 100)
        );
        milestone.Activate();
        milestone.Complete();
        milestone.ClaimReward();

        Assert.NotNull(milestone.RewardClaimedAt);
        Assert.Throws<InvalidOperationException>(milestone.ClaimReward);
    }
}
