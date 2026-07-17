namespace LevelUp.Infrastructure.Persistence;

public static class LevelUpPaths
{
    public static string GetDefaultDataDirectory()
    {
        string? configured = Environment.GetEnvironmentVariable("LEVELUP_DATA_DIR");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            Directory.CreateDirectory(configured);
            return Path.GetFullPath(configured);
        }

        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LevelUp"
        );
        Directory.CreateDirectory(directory);
        return directory;
    }

    public static string GetDefaultDatabasePath() => Path.Combine(GetDefaultDataDirectory(), "levelup.db");

}
