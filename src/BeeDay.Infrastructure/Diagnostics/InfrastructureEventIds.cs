using Microsoft.Extensions.Logging;

namespace BeeDay.Infrastructure.Diagnostics;

public static class InfrastructureEventIds
{
    public static readonly EventId DataFileCreated = new(6001, nameof(DataFileCreated));
    public static readonly EventId DataFileLoaded = new(6002, nameof(DataFileLoaded));
    public static readonly EventId DataFileSaved = new(6003, nameof(DataFileSaved));
    public static readonly EventId DataFileInvalid = new(6004, nameof(DataFileInvalid));
    public static readonly EventId BackupCreated = new(6010, nameof(BackupCreated));
    public static readonly EventId BackupRemoved = new(6011, nameof(BackupRemoved));
    public static readonly EventId BackupInvalid = new(6012, nameof(BackupInvalid));
    public static readonly EventId BackupRestored = new(6013, nameof(BackupRestored));
}
