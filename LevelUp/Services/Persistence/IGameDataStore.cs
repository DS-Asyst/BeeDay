using LevelUp.Domain;

namespace LevelUp.Services.Persistence;

public interface IGameDataStore
{
    GameData? Load();
    void Save(GameData gameData);
}
