using LevelUp.Domain.Achievements;
using LevelUp.Services.Achievements;
using Xunit;

namespace LevelUp.Tests;

public sealed class ReadingAchievementTests
{
    [Fact]
    public void FirstCompletedBookUnlocksPrimeirasPaginasOnlyOnce()
    {
        AchievementService service = new();
        var first = service.UnlockReadingAchievements(1);
        var repeated = service.UnlockReadingAchievements(1);
        Assert.Single(first);
        Assert.Empty(repeated);
        Assert.Equal("First Pages", service.GetUnlocked().Single().Name);
    }

    [Fact]
    public void TenCompletedBooksUnlockAllReachedReadingMilestones()
    {
        AchievementService service = new();
        service.UnlockReadingAchievements(10);
        Assert.Equal(3, service.GetUnlocked().Count);
        Assert.All(service.GetUnlocked(), item => Assert.Equal(AchievementCategory.Reading, item.Category));
    }
}
