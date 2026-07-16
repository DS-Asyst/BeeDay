namespace LevelUp.Domain.Character;

public static class CharacterRankResolver
{
    public static CharacterRank Resolve(int level)
    {
        if (level <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }

        return level switch
        {
            <= 9 => CharacterRank.Apprentice,
            <= 19 => CharacterRank.Adventurer,
            <= 29 => CharacterRank.Disciple,
            <= 39 => CharacterRank.Adept,
            <= 49 => CharacterRank.Specialist,
            <= 59 => CharacterRank.Master,
            _ => CharacterRank.Legend
        };
    }
}
