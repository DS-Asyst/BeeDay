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
            $"Conquista obtida ao concluir o projeto {project.Name}.",
            AchievementCategory.Project,
            project.Id
        );
        achievement.Unlock();
        achievements.Add(achievement);
        return achievement;
    }
}
