using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Services.Bosses;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using Xunit;

namespace LevelUp.Tests;

public sealed class MilestoneAndBossTests
{
    [Fact]
    public void OnlyOneMilestoneCanBeActivePerProject()
    {
        ProjectService projects = new();
        MilestoneService milestones = new();
        var project = projects.CreateProject("Project", "Description");
        projects.ActivateProject(project);
        var first = milestones.CreateMilestone(project, "First", "Description", 1);
        var second = milestones.CreateMilestone(project, "Second", "Description", 2);
        milestones.Activate(first);

        Assert.Throws<InvalidOperationException>(() => milestones.Activate(second));
    }

    [Fact]
    public void ProjectCanHaveOnlyOneFinalBoss()
    {
        ProjectService projects = new();
        BossService bosses = new();
        var project = projects.CreateProject("Project", "Description");
        bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");

        Assert.Throws<InvalidOperationException>(() =>
            bosses.CreateFinalBoss(project, "Other", "Description", "Specialist")
        );
    }

    [Fact]
    public void FinalBossStartsLocked()
    {
        ProjectService projects = new();
        BossService bosses = new();
        var project = projects.CreateProject("Project", "Description");
        var boss = bosses.CreateFinalBoss(project, "Boss", "Description", "Developer");

        Assert.Equal(BossStatus.Locked, boss.Status);
        Assert.Equal("Developer", boss.AchievementPrefix);
    }
}
