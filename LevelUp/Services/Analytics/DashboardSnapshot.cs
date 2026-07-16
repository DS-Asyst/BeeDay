using LevelUp.Domain.Character;

namespace LevelUp.Services.Analytics;

public sealed record DashboardSnapshot(
    int Level,
    CharacterRank Rank,
    decimal Experience,
    decimal ExperienceToNextLevel,
    int ActiveProjects,
    int ActiveQuests,
    int CompletedQuests,
    int ActiveBooks,
    int PagesReadThisMonth,
    decimal WalletBalance,
    decimal WalletMonthResult,
    int UnlockedAchievements
);
