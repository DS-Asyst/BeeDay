namespace LevelUp.Infrastructure.Persistence.Exceptions;

public sealed class BackupRestoreException : PersistenceException
{
    public BackupRestoreException(string message, Exception innerException) : base(message, innerException) { }
}
