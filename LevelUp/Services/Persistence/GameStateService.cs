using LevelUp.Application;
using LevelUp.Domain;

namespace LevelUp.Services.Persistence;

public sealed class GameStateService
{
    private readonly IGameDataStore dataStore;
    private readonly GameSession session;

    public GameStateService(IGameDataStore dataStore, GameSession session)
    {
        this.dataStore = dataStore;
        this.session = session;
    }

    public GameData CreateSnapshot() => new()
    {
        SchemaVersion = GameData.CurrentSchemaVersion,
        Character = session.Character,
        Habits = session.Habits.GetAllHabits().ToList(),
        Tasks = session.Tasks.GetAll().ToList(),
        Projects = session.Projects.GetAllProjects().ToList(),
        Todos = session.Todos.GetAll().ToList(),
        Milestones = session.Milestones.GetAll().ToList(),
        Bosses = session.Bosses.GetAll().ToList(),
        Books = session.Books.GetAll().ToList(),
        WalletTransactions = session.Wallet.GetAll().ToList(),
        Achievements = session.Achievements.GetAll().ToList(),
        WalletTags = session.Wallet.GetAllTags().ToList(),
        SaveRevision = session.SaveRevision,
        LastSavedAt = session.LastSavedAt
    };

    public int CurrentSaveRevision => session.SaveRevision;
    public DateTime? LastSavedAt => session.LastSavedAt;

    public void Save()
    {
        int previousRevision = session.SaveRevision;
        DateTime? previousSavedAt = session.LastSavedAt;
        session.SaveRevision++;
        session.LastSavedAt = DateTime.Now;
        try { dataStore.Save(CreateSnapshot()); }
        catch
        {
            session.SaveRevision = previousRevision;
            session.LastSavedAt = previousSavedAt;
            throw;
        }
    }
}
