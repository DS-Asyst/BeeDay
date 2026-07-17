using LevelUp.Domain.Character;
using Xunit;

namespace LevelUp.Tests;

public sealed class CharacterRankTests
{
    [Theory]
    [InlineData(1, CharacterRank.Apprentice)]
    [InlineData(10, CharacterRank.Adventurer)]
    [InlineData(20, CharacterRank.Disciple)]
    [InlineData(30, CharacterRank.Adept)]
    [InlineData(40, CharacterRank.Specialist)]
    [InlineData(50, CharacterRank.Master)]
    [InlineData(60, CharacterRank.Legend)]
    public void CharacterRank_ShouldFollowProgressionBands(
        int level,
        CharacterRank expected
    )
    {
        Assert.Equal(
            expected,
            CharacterRankResolver.Resolve(level)
        );
    }
}
