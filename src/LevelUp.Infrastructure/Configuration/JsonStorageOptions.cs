namespace LevelUp.Infrastructure.Configuration;

public sealed class JsonStorageOptions
{
    public const string SectionName = "LevelUp:Storage";

    public string Directory { get; set; } = "Data";
    public string FileName { get; set; } = "LevelUpBD.json";
    public string BackupDirectory { get; set; } = "Backups";
    public int BackupRetention { get; set; } = 10;
    public bool CreateBackupBeforeSave { get; set; } = true;
    public bool RecoverFromBackup { get; set; } = true;
    public bool WriteIndented { get; set; } = true;
}
