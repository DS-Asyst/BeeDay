using LevelUp.Domain.Achievements;
using LevelUp.Domain.Books;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Tasks;
using LevelUp.Domain.Todos;
using LevelUp.Domain.Wallet;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Domain;

public class GameData
{
    public const int CurrentSchemaVersion = 7;

    public int SchemaVersion { get; set; }
    public int SaveRevision { get; set; }
    public DateTime? LastSavedAt { get; set; }
    public CharacterModel Character { get; set; } = new();
    public List<Habit> Habits { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
    /// <summary>
    /// Temporary Console compatibility snapshot. New schema features must use Tasks and Todos.
    /// This replaces the removed GameData.Quests API and is isolated from new domain code.
    /// </summary>
    public List<Quest> LegacyQuests { get; set; } = [];
    public List<TaskItem> Tasks { get; set; } = [];
    public List<ProjectTodo> Todos { get; set; } = [];
    public List<Milestone> Milestones { get; set; } = [];
    public List<BossEncounter> Bosses { get; set; } = [];
    public List<Book> Books { get; set; } = [];
    public List<WalletTag> WalletTags { get; set; } = [];
    public List<WalletTransaction> WalletTransactions { get; set; } = [];
    public List<Achievement> Achievements { get; set; } = [];
}
