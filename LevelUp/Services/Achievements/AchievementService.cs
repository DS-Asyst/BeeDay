using LevelUp.Domain.Achievements;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Projects;

namespace LevelUp.Services.Achievements;

public sealed class AchievementService
{
    private readonly List<Achievement> achievements = [];
    private int nextId = 1;

    public AchievementService(IEnumerable<Achievement>? achievements = null)
    {
        if (achievements is null)
        {
            return;
        }

        this.achievements.AddRange(achievements);
        if (this.achievements.Count > 0)
        {
            nextId = this.achievements.Max(item => item.Id) + 1;
        }
    }

    public IReadOnlyList<Achievement> GetAll()
    {
        return achievements.AsReadOnly();
    }

    public IReadOnlyList<Achievement> GetUnlocked()
    {
        return achievements
            .Where(item => item.Status == AchievementStatus.Unlocked)
            .OrderByDescending(item => item.UnlockedAt)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Achievement> UnlockReadingAchievements(int completedBooks)
    {
        var definitions = new (int Count, string Code, string Name, string Description)[]
        {
            (1, "reading:1", "First Pages", "A new story began: your first book was completed."),
            (5, "reading:5", "Turning Pages", "Five completed books and many pages are now part of your journey."),
            (10, "reading:10", "Between Stories", "Ten completed books: your collection of experiences keeps growing."),
            (25, "reading:25", "Story Collector", "Twenty-five completed books, each leaving a new perspective."),
            (50, "reading:50", "Living Library", "Fifty completed books: stories and knowledge are now part of who you are.")
        };
        List<Achievement> unlocked = [];
        foreach (var definition in definitions.Where(item => completedBooks >= item.Count))
        {
            Achievement? achievement = achievements.FirstOrDefault(item => item.Code == definition.Code);
            if (achievement is null)
            {
                achievement = new Achievement { Id = nextId++ };
                achievement.Configure(definition.Code, definition.Name, definition.Description, AchievementCategory.Reading);
                achievements.Add(achievement);
            }
            bool wasLocked = achievement.Status == AchievementStatus.Locked;
            achievement.Unlock();
            if (wasLocked) unlocked.Add(achievement);
        }
        return unlocked.AsReadOnly();
    }

    public Achievement UnlockProjectAchievement(
        Project project,
        BossEncounter boss
    )
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(boss);

        string code = $"project:{project.Id}";
        Achievement? existing = achievements.FirstOrDefault(
            item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)
        );

        if (existing is not null)
        {
            existing.Unlock();
            return existing;
        }

        string name = $"{boss.AchievementPrefix} {boss.Name}".Trim();
        Achievement achievement = new() { Id = nextId++ };
        achievement.Configure(
            code,
            name,
            $"Achievement earned by completing the project {project.Name}.",
            AchievementCategory.Project,
            project.Id
        );
        achievement.Unlock();
        achievements.Add(achievement);
        return achievement;
    }
}
