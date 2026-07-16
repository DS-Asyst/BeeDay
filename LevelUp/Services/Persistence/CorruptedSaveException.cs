namespace LevelUp.Services.Persistence;

public sealed class CorruptedSaveException : Exception
{
    public CorruptedSaveException(
        string backupPath,
        Exception innerException
    )
        : base(
            "O arquivo de salvamento é incompatível ou está corrompido.",
            innerException
        )
    {
        BackupPath = backupPath;
    }

    public string BackupPath { get; }
}
