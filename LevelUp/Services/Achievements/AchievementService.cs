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
            (1, "reading:1", "Primeiras Páginas", "Uma nova história começou: seu primeiro livro foi concluído."),
            (5, "reading:5", "Virando Capítulos", "Cinco livros concluídos e muitas páginas já fazem parte da sua jornada."),
            (10, "reading:10", "Entre Histórias", "Dez livros concluídos: sua coleção de experiências continua crescendo."),
            (25, "reading:25", "Colecionador de Histórias", "Vinte e cinco livros concluídos, cada um deixando uma nova perspectiva."),
            (50, "reading:50", "Biblioteca Viva", "Cinquenta livros concluídos: histórias e conhecimento já fazem parte de quem você é.")
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
            $"Conquista obtida ao concluir o projeto {project.Name}.",
            AchievementCategory.Project,
            project.Id
        );
        achievement.Unlock();
        achievements.Add(achievement);
        return achievement;
    }
}
