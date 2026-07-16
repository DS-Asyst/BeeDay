using LevelUp.Domain.Achievements;
using LevelUp.Domain.Character;
using LevelUp.Services.Achievements;
using LevelUp.Services.Bosses;
using LevelUp.Services.Projects;
using Xunit;

namespace LevelUp.Tests;

public sealed class RecognitionTests
{
    [Theory]
    [InlineData(1, CharacterRank.Apprentice)]
    [InlineData(10, CharacterRank.Adventurer)]
    [InlineData(20, CharacterRank.Disciple)]
    [InlineData(30, CharacterRank.Adept)]
    [InlineData(40, CharacterRank.Specialist)]
    [InlineData(50, CharacterRank.Master)]
    [InlineData(60, CharacterRank.Legend)]
    public void RankFollowsLevelRange(int level, CharacterRank expected)
    {
        Assert.Equal(expected, CharacterRankResolver.Resolve(level));
    }

    [Fact]
    public void ProjectAchievementIsUnique()
    {
        ProjectService projects = new();
        BossService bosses = new();
        AchievementService achievements = new();
        var project = projects.CreateProject("Project", "Description");
        var boss = bosses.CreateFinalBoss(project, "ASP.NET Core", "Description", "Developer");

        var first = achievements.UnlockProjectAchievement(project, boss);
        var second = achievements.UnlockProjectAchievement(project, boss);

        Assert.Equal(first.Id, second.Id);
        Assert.Single(achievements.GetAll());
        Assert.Equal(AchievementStatus.Unlocked, first.Status);
    }
}
