using System.Text.Json;
using LevelUp.Domain;
using LevelUp.Domain.Character;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Projects;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public class SaveService
{
    private readonly string filePath;

    public SaveService()
    {
        string projectDirectory = FindProjectDirectory();


        string dataDirectory = Path.Combine(
            projectDirectory,
            "Data"
        );

        Directory.CreateDirectory(dataDirectory);

        filePath = Path.Combine(
            dataDirectory,
            "save.json"
        );



    }

    private static string FindProjectDirectory()
    {
        DirectoryInfo? directory = new DirectoryInfo(
            AppContext.BaseDirectory
        );

        while (directory is not null)
        {
            bool hasProjectFile = directory
                .GetFiles("*.csproj")
                .Length > 0;

            if (hasProjectFile)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar a pasta do projeto."
        );
    }

    private string CreateBackup()
    {
        string? dataDirectory =
            Path.GetDirectoryName(filePath);

        if (dataDirectory is null)
        {
            throw new DirectoryNotFoundException(
                "Não foi possível localizar a pasta do arquivo de salvamento."
            );
        }

        string backupFileName =
            $"save_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";

        string backupPath = Path.Combine(
            dataDirectory,
            backupFileName
        );

        File.Copy(
            filePath,
            backupPath,
            overwrite: true
        );

        return backupPath;
    }

    public void SaveGame(GameData gameData)
    {
        ArgumentNullException.ThrowIfNull(gameData);

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(
            gameData,
            options
        );

        File.WriteAllText(filePath, json);
    }

    public GameData? LoadGame()
    {

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);

            GameData? gameData =
                JsonSerializer.Deserialize<GameData>(json);

            if (gameData is null)
            {
                return null;
            }

            gameData.Character ??= new CharacterModel();
            gameData.Habits ??= new List<Habit>();
            gameData.Projects ??= new List<Project>();

            return gameData;
        }
        catch (JsonException)
        {
            string backupPath = CreateBackup();

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(
                "O arquivo de salvamento é incompatível ou está corrompido."
            );

            Console.WriteLine(
                $"Um backup foi criado em: {backupPath}"
            );

            Console.WriteLine(
                "Um novo jogo será iniciado."
            );

            Console.ResetColor();

            return null;
        }
    }
}