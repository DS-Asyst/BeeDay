using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Application;

public sealed class GameSession
{
    public GameSession(
        CharacterModel character,
        HabitService habits,
        ProjectService projects,
        QuestService quests,
        MilestoneService milestones,
        BossService bosses,
        BookService books,
        WalletService wallet,
        AchievementService achievements,
        int saveRevision = 0,
        DateTime? lastSavedAt = null
    )
    {
        Character = character;
        Habits = habits;
        Projects = projects;
        Quests = quests;
        Milestones = milestones;
        Bosses = bosses;
        Books = books;
        Wallet = wallet;
        Achievements = achievements;
        SaveRevision = saveRevision;
        LastSavedAt = lastSavedAt;
    }

    public CharacterModel Character { get; }
    public HabitService Habits { get; }
    public ProjectService Projects { get; }
    public QuestService Quests { get; }
    public MilestoneService Milestones { get; }
    public BossService Bosses { get; }
    public BookService Books { get; }
    public WalletService Wallet { get; }
    public AchievementService Achievements { get; }
    public int SaveRevision { get; internal set; }
    public DateTime? LastSavedAt { get; internal set; }
}
