using System.Text.Json;
using LevelUp.Domain;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public sealed class SaveService : IGameDataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string filePath;

    public SaveService()
        : this(GetDefaultFilePath())
    {
    }

    public SaveService(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        this.filePath = Path.GetFullPath(filePath);

        string? directory = Path.GetDirectoryName(this.filePath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }
    }

    public string FilePath => filePath;

    public void Save(GameData gameData)
    {
        ArgumentNullException.ThrowIfNull(gameData);

        string json = JsonSerializer.Serialize(
            gameData,
            SerializerOptions
        );

        File.WriteAllText(filePath, json);
    }

    public GameData? Load()
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            GameData? gameData = JsonSerializer.Deserialize<GameData>(json);

            if (gameData is null)
            {
                return null;
            }

            gameData.Character ??= new CharacterModel();
            gameData.Habits ??= [];
            gameData.Projects ??= [];
            gameData.Quests ??= [];

            return gameData;
        }
        catch (JsonException exception)
        {
            string backupPath = CreateBackup();
            throw new CorruptedSaveException(backupPath, exception);
        }
    }

    // Compatibility wrappers for existing callers.
    public void SaveGame(GameData gameData) => Save(gameData);
    public GameData? LoadGame() => Load();

    private string CreateBackup()
    {
        string directory = Path.GetDirectoryName(filePath)
            ?? throw new DirectoryNotFoundException(
                "The save directory could not be located."
            );

        string backupPath = Path.Combine(
            directory,
            $"save_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        );

        File.Copy(filePath, backupPath, overwrite: true);
        return backupPath;
    }

    private static string GetDefaultFilePath()
    {
        string projectDirectory = FindProjectDirectory();
        return Path.Combine(projectDirectory, "Data", "save.json");
    }

    private static string FindProjectDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (directory.GetFiles("*.csproj").Length > 0)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "The project directory could not be located."
        );
    }
}
