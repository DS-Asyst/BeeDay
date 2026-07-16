namespace LevelUp.Domain.Milestones;

public sealed record MilestoneReward(
    int Experience = 0,
    int Gold = 0,
    string? Title = null
)
{
    public bool HasReward =>
        Experience > 0 ||
        Gold > 0 ||
        !string.IsNullOrWhiteSpace(Title);
}
