using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Diagnostics;
using LevelUp.Infrastructure.Persistence.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonBackupService(
    JsonStoragePaths paths,
    IOptions<JsonStorageOptions> options,
    JsonFileReader reader,
    ILogger<JsonBackupService> logger)
{
    public async Task CreateAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.CreateBackupBeforeSave || !File.Exists(paths.DataFile))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(paths.BackupDirectory);
            var backupPath = paths.CreateBackupFile(DateTimeOffset.UtcNow);

            await using (var source = new FileStream(
                paths.DataFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16 * 1024,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var destination = new FileStream(
                backupPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16 * 1024,
                options: FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await source.CopyToAsync(destination, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }

            // Validate only after both streams have been disposed.
            await reader.ReadAsync(backupPath, cancellationToken);

            logger.LogDebug(
                InfrastructureEventIds.BackupCreated,
                "JSON backup created. BackupPath: {BackupPath}",
                backupPath);
            Prune();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or PersistenceException)
        {
            throw new BackupRestoreException("The JSON backup could not be created.", exception);
        }
    }

    public async Task<(LevelUp.Domain.Entities.LevelUpData Data, string Path)> ReadLatestValidAsync(
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(paths.BackupDirectory))
        {
            throw new BackupRestoreException(
                "No JSON backup directory is available.",
                new DirectoryNotFoundException(paths.BackupDirectory));
        }

        var failures = new List<Exception>();
        foreach (var backup in new DirectoryInfo(paths.BackupDirectory)
                     .GetFiles("*.json")
                     .OrderByDescending(file => file.Name))
        {
            try
            {
                return (await reader.ReadAsync(backup.FullName, cancellationToken), backup.FullName);
            }
            catch (PersistenceException exception)
            {
                failures.Add(exception);
                logger.LogWarning(
                    InfrastructureEventIds.BackupInvalid,
                    exception,
                    "Skipping invalid JSON backup. BackupPath: {BackupPath}",
                    backup.FullName);
            }
        }

        throw new BackupRestoreException(
            "No valid JSON backup could be found.",
            new AggregateException(failures));
    }

    private void Prune()
    {
        var retention = options.Value.BackupRetention;
        foreach (var oldBackup in new DirectoryInfo(paths.BackupDirectory)
                     .GetFiles("*.json")
                     .OrderByDescending(file => file.Name)
                     .Skip(retention))
        {
            oldBackup.Delete();
            logger.LogDebug(
                InfrastructureEventIds.BackupRemoved,
                "Expired JSON backup removed. BackupPath: {BackupPath}",
                oldBackup.FullName);
        }
    }
}
