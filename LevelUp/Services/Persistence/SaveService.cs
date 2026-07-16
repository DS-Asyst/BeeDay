using System.Text.Json;
using LevelUp.Domain;
using LevelUp.Services.Persistence.Migrations;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public sealed class SaveService : IGameDataStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string filePath;
    private readonly GameDataMigrator migrator;
    private readonly GameDataValidator validator;

    public SaveService()
        : this(GetDefaultFilePath())
    {
    }

    public SaveService(
        string filePath,
        GameDataMigrator? migrator = null,
        GameDataValidator? validator = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        this.filePath = Path.GetFullPath(filePath);
        this.migrator = migrator ?? new GameDataMigrator();
        this.validator = validator ?? new GameDataValidator();
        string? directory = Path.GetDirectoryName(this.filePath);
        if (directory is not null) Directory.CreateDirectory(directory);
    }

    public string FilePath => filePath;

    public void Save(GameData gameData)
    {
        ArgumentNullException.ThrowIfNull(gameData);
        gameData.SchemaVersion = GameData.CurrentSchemaVersion;
        validator.Validate(gameData);

        string json = JsonSerializer.Serialize(gameData, SerializerOptions);
        string temporaryPath = filePath + ".tmp";
        string previousPath = filePath + ".previous";

        File.WriteAllText(temporaryPath, json);
        using (FileStream stream = new(temporaryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            stream.Flush(flushToDisk: true);
        }

        if (File.Exists(filePath))
        {
            File.Copy(filePath, previousPath, overwrite: true);
        }

        File.Move(temporaryPath, filePath, overwrite: true);
    }

    public GameData? Load()
    {
        if (!File.Exists(filePath)) return null;
        try
        {
            string json = File.ReadAllText(filePath);
            GameData? gameData = JsonSerializer.Deserialize<GameData>(json);
            if (gameData is null) return null;
            Normalize(gameData);
            migrator.Migrate(gameData);
            validator.Validate(gameData);
            return gameData;
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidDataException or InvalidOperationException
        )
        {
            string backupPath = CreateBackup();
            throw new CorruptedSaveException(backupPath, exception);
        }
    }

    public void SaveGame(GameData gameData) => Save(gameData);
    public GameData? LoadGame() => Load();

    private static void Normalize(GameData gameData)
    {
        gameData.Character ??= new CharacterModel();
        gameData.Habits ??= [];
        gameData.Projects ??= [];
        gameData.Quests ??= [];
        gameData.Milestones ??= [];
        gameData.Bosses ??= [];
        gameData.Books ??= [];
        gameData.WalletTransactions ??= [];
        gameData.Achievements ??= [];

        foreach (var book in gameData.Books)
        {
            book.ProgressHistory ??= [];
        }
    }

    private string CreateBackup()
    {
        string directory = Path.GetDirectoryName(filePath)
            ?? throw new DirectoryNotFoundException("Não foi possível localizar a pasta de salvamento.");
        string backupPath = Path.Combine(directory, $"save_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
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
            if (directory.GetFiles("*.csproj").Length > 0) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Não foi possível localizar a pasta do projeto.");
    }
}
