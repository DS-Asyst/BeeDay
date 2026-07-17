using LevelUp.Application;
using LevelUp.Domain;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Persistence;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using LevelUp.Services.Wallet;
using Xunit;

namespace LevelUp.Tests;

public sealed class SaveMetadataTests
{
    [Fact]
    public void Save_ShouldIncrementRevisionAndSetTimestamp()
    {
        InMemoryStore store = new();
        GameSession session = new(
            new LevelUp.Domain.Character.Character(),
            new HabitService(),
            new ProjectService(),
            new QuestService(),
            new MilestoneService(),
            new BossService(),
            new BookService(),
            new WalletService(),
            new AchievementService(),
            saveRevision: 7
        );
        GameStateService state = new(store, session);

        state.Save();

        Assert.Equal(8, state.CurrentSaveRevision);
        Assert.NotNull(state.LastSavedAt);
        Assert.Equal(8, store.Data?.SaveRevision);
        Assert.NotNull(store.Data?.LastSavedAt);
    }

    private sealed class InMemoryStore : IGameDataStore
    {
        public GameData? Data { get; private set; }
        public GameData? Load() => Data;
        public void Save(GameData gameData) => Data = gameData;
    }
}
