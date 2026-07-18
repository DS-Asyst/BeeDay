using LevelUp.Domain.Attributes;
using LevelUp.Domain.Rewards;

namespace LevelUp.Domain.Character;

public class Character : ILevelProgress
{
    public string Name { get; set; } = string.Empty;

    public CharacterClass Class { get; set; } = CharacterClass.Warrior;

    public int Level { get; set; } = 1;

    public decimal Experience { get; set; }

    public PlayerAttributes Attributes { get; set; } = new();

    public decimal ExperienceToNextLevel => Level * 100m;

    public CharacterRank Rank => CharacterRankResolver.Resolve(Level);

    public List<string> Titles { get; set; } = [];

    public void ApplyReward(Reward reward)
    {
        ArgumentNullException.ThrowIfNull(reward);
        ApplyProgress(this, reward.Experience);

        if (reward.Attribute is AttributeType attribute && reward.AttributeExperience != 0m)
        {
            ApplyProgress(Attributes.GetAttribute(attribute), reward.AttributeExperience);
        }

        foreach (string title in reward.Titles ?? [])
        {
            if (!Titles.Any(existing => string.Equals(existing, title, StringComparison.OrdinalIgnoreCase)))
            {
                Titles.Add(title);
            }
        }
    }

    private static void ApplyProgress(ILevelProgress progress, decimal experience)
    {
        if (experience < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(experience), "A reward cannot remove experience.");
        }

        progress.Experience += experience;
        while (progress.Experience >= progress.ExperienceToNextLevel)
        {
            decimal required = progress.ExperienceToNextLevel;
            progress.Experience -= required;
            progress.Level++;
        }
    }
}
