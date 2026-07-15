namespace LevelUp.Services.Persistence;

public sealed class CorruptedSaveException : Exception
{
    public CorruptedSaveException(
        string backupPath,
        Exception innerException
    )
        : base(
            "The save file is incompatible or corrupted.",
            innerException
        )
    {
        BackupPath = backupPath;
    }

    public string BackupPath { get; }
}
