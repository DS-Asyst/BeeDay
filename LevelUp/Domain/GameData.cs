using LevelUp.Domain.Achievements;
using LevelUp.Domain.Books;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Wallet;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Domain;

public class GameData
{
    public const int CurrentSchemaVersion = 4;

    public int SchemaVersion { get; set; }
    public CharacterModel Character { get; set; } = new();
    public List<Habit> Habits { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
    public List<Quest> Quests { get; set; } = [];
    public List<Milestone> Milestones { get; set; } = [];
    public List<BossEncounter> Bosses { get; set; } = [];
    public List<Book> Books { get; set; } = [];
    public List<WalletTransaction> WalletTransactions { get; set; } = [];
    public List<Achievement> Achievements { get; set; } = [];
}
