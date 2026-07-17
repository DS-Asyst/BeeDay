using LevelUp.Domain.Attributes;
using LevelUp.Domain.Character;
using LevelUp.Domain.Rewards;
using Xunit;

namespace LevelUp.Tests.Rewards;

public sealed class RewardRegressionTests
{
    [Fact]
    public void ApplyReward_ShouldBeTheOnlyCharacterMutationEntryPoint()
    {
        Character character = new();
        Reward reward = new(1m, AttributeType.Intelligence, 1m, ["ASP.NET Core Apprentice"]);

        character.ApplyReward(reward);

        Assert.Equal(1m, character.Experience);
        Assert.Equal(1m, character.Attributes.Intelligence.Experience);
        Assert.Contains("ASP.NET Core Apprentice", character.Titles);
    }

    [Fact]
    public void ChapterReward_ShouldEqualSumOfQuestExperience()
    {
        Reward total = Reward.None;
        total = total.Add(new Reward(1m, AttributeType.Intelligence, 1m));
        total = total.Add(new Reward(2m, AttributeType.Intelligence, 2m));

        Assert.Equal(3m, total.Experience);
        Assert.Equal(3m, total.AttributeExperience);
    }
}
