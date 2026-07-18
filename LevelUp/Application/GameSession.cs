using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Tasks;
using LevelUp.Services.Todos;
using LevelUp.Services.Wallet;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Application;

public sealed class GameSession
{
    public GameSession(
        CharacterModel character,
        HabitService habits,
        TaskService tasks,
        ProjectService projects,
        ProjectTodoService todos,
        MilestoneService milestones,
        BossService bosses,
        BookService books,
        WalletService wallet,
        AchievementService achievements,
        int saveRevision = 0,
        DateTime? lastSavedAt = null)
    {
        Character = character;
        Habits = habits;
        Tasks = tasks;
        Projects = projects;
        Todos = todos;
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
    public TaskService Tasks { get; }
    public ProjectService Projects { get; }
    public ProjectTodoService Todos { get; }
    public MilestoneService Milestones { get; }
    public BossService Bosses { get; }
    public BookService Books { get; }
    public WalletService Wallet { get; }
    public AchievementService Achievements { get; }
    public int SaveRevision { get; internal set; }
    public DateTime? LastSavedAt { get; internal set; }
}
