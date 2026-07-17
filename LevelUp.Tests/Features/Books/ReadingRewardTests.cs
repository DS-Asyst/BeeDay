using LevelUp.Services.Workflows;
using Xunit;

namespace LevelUp.Tests;

public sealed class ReadingRewardTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 10)]
    [InlineData(273, 27)]
    public void CompletionRewardUsesBookPageRule(int totalPages, decimal expectedExperience)
    {
        var reward = ReadingWorkflowService.CreateCompletionReward(totalPages);
        Assert.Equal(expectedExperience, reward.Experience);
    }
}
