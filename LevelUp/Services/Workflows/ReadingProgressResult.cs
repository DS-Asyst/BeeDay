using LevelUp.Domain.Books;
using LevelUp.Domain.Rewards;

namespace LevelUp.Services.Workflows;

public sealed record ReadingProgressResult(
    Book Book,
    int PagesRead,
    Reward Reward,
    bool BookCompleted
)
{
    public decimal ExperienceEarned => Reward.Experience;
}
