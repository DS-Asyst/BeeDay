using LevelUp.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonStoragePaths
{
    public JsonStoragePaths(IHostEnvironment environment, IOptions<JsonStorageOptions> options)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);

        var root = Path.GetFullPath(environment.ContentRootPath);
        StorageDirectory = ResolveDirectory(root, options.Value.Directory, "storage directory");
        DataFile = ResolveFile(StorageDirectory, options.Value.FileName);
        BackupDirectory = ResolveChild(StorageDirectory, options.Value.BackupDirectory, "backup directory");
    }

    public string StorageDirectory { get; }
    public string DataFile { get; }
    public string BackupDirectory { get; }

    public string CreateTemporaryFile() => Path.Combine(
        StorageDirectory,
        $".{Path.GetFileName(DataFile)}.{Guid.NewGuid():N}.tmp");

    public string CreateRecoveryFile() => Path.Combine(
        StorageDirectory,
        $".{Path.GetFileName(DataFile)}.{Guid.NewGuid():N}.recovered");

    public string CreateBackupFile(DateTimeOffset timestamp) => Path.Combine(
        BackupDirectory,
        $"{Path.GetFileNameWithoutExtension(DataFile)}-{timestamp.UtcDateTime:yyyyMMdd-HHmmssfff}-{Guid.NewGuid():N}.json");

    private static string ResolveDirectory(string parent, string configuredPath, string label)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        return ResolveChild(parent, configuredPath, label);
    }

    private static string ResolveChild(string parent, string child, string label)
    {
        var resolved = Path.GetFullPath(Path.Combine(parent, child));
        var prefix = parent.EndsWith(Path.DirectorySeparatorChar)
            ? parent
            : parent + Path.DirectorySeparatorChar;

        if (!resolved.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(resolved, parent, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"The {label} must remain inside '{parent}'.");
        }

        return resolved;
    }

    private static string ResolveFile(string directory, string fileName)
    {
        if (!string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The JSON storage file name cannot contain a directory path.");
        }

        return Path.Combine(directory, fileName);
    }
}
