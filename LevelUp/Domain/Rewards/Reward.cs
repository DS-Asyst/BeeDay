using LevelUp.Domain.Attributes;

namespace LevelUp.Domain.Rewards;

public sealed record Reward(
    decimal Experience = 0m,
    AttributeType? Attribute = null,
    decimal AttributeExperience = 0m,
    IReadOnlyCollection<string>? Titles = null
)
{
    public static Reward None { get; } = new();

    public bool IsEmpty =>
        Experience == 0m &&
        AttributeExperience == 0m &&
        (Titles is null || Titles.Count == 0);

    public Reward Add(Reward other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Attribute is not null && other.Attribute is not null && Attribute != other.Attribute)
        {
            throw new InvalidOperationException("Rewards for different attributes cannot be combined.");
        }

        return new Reward(
            Experience + other.Experience,
            Attribute ?? other.Attribute,
            AttributeExperience + other.AttributeExperience,
            (Titles ?? []).Concat(other.Titles ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        );
    }
}
