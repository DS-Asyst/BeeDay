using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.UI.Infrastructure;
using Xunit;

namespace LevelUp.Tests.UI;

public sealed class DisplayTextTests
{
    [Fact]
    public void ProjectStatus_ShouldBeDisplayedInPortuguese()
    {
        Assert.Equal("Completed", DisplayText.For(ProjectStatus.Completed));
    }

    [Fact]
    public void QuestStatus_ShouldUseFemininePortugueseLabel()
    {
        Assert.Equal("Completed", DisplayText.For(QuestStatus.Completed));
    }

    [Fact]
    public void MilestoneStatus_ShouldUseChapterLanguage()
    {
        Assert.Equal("Locked", DisplayText.For(MilestoneStatus.Locked));
    }

    [Fact]
    public void BossStatus_ShouldBeDisplayedInPortuguese()
    {
        Assert.Equal("Defeated", DisplayText.For(BossStatus.Defeated));
    }
}
