using Xunit;
using LevelUp.Domain;
using LevelUp.Domain.Habits;
using LevelUp.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace LevelUp.Tests.Features.Infrastructure;

public sealed class SqliteGameDataStoreTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"levelup-sqlite-{Guid.NewGuid():N}");

    [Fact]
    public void SaveAndLoad_PreserveGameSnapshot()
    {
        Directory.CreateDirectory(root);
        string database = Path.Combine(root, "levelup.db");
        GameData expected = CreateGameData("Tiago");

        using (SqliteGameDataStore store = new(database))
        {
            store.Save(expected);
        }

        using SqliteGameDataStore reloadedStore = new(database);
        GameData? actual = reloadedStore.Load();

        Assert.NotNull(actual);
        Assert.Equal(expected.Character.Name, actual.Character.Name);
        Assert.Single(actual.Habits);
        Assert.Equal("Estudar arquitetura", actual.Habits[0].Title);
    }

    [Fact]
    public void InitialMigration_CreatesRelationalTables()
    {
        Directory.CreateDirectory(root);
        string database = Path.Combine(root, "schema.db");
        using (SqliteGameDataStore store = new(database))
        {
        }

        using SqliteConnection connection = new($"Data Source={database}");
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name";
        using SqliteDataReader reader = command.ExecuteReader();
        List<string> tables = [];
        while (reader.Read()) tables.Add(reader.GetString(0));

        Assert.Contains("GameMetadata", tables);
        Assert.Contains("Characters", tables);
        Assert.Contains("Projects", tables);
        Assert.Contains("Books", tables);
        Assert.Contains("WalletTransactions", tables);
        Assert.Contains("CharacterTitles", tables);
        Assert.Contains("BookProgressEntries", tables);
        Assert.Contains("__EFMigrationsHistory", tables);

        using SqliteCommand columnsCommand = connection.CreateCommand();
        columnsCommand.CommandText = "PRAGMA table_info(Projects)";
        using SqliteDataReader columnsReader = columnsCommand.ExecuteReader();
        List<string> columns = [];
        while (columnsReader.Read()) columns.Add(columnsReader.GetString(1));

        Assert.Contains("Name", columns);
        Assert.Contains("PrimaryAttribute", columns);
        Assert.DoesNotContain("Payload", columns);
    }

    [Fact]
    public void Load_ReturnsNull_WhenDatabaseHasNoGame()
    {
        Directory.CreateDirectory(root);
        using SqliteGameDataStore store = new(Path.Combine(root, "empty.db"));

        Assert.Null(store.Load());
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private static GameData CreateGameData(string characterName)
    {
        return new GameData
        {
            SchemaVersion = GameData.CurrentSchemaVersion,
            SaveRevision = 3,
            LastSavedAt = DateTime.Now,
            Character = new LevelUp.Domain.Character.Character { Name = characterName },
            Habits = [new Habit { Id = 1, Title = "Estudar arquitetura" }]
        };
    }
}
