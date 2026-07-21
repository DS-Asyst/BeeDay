using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Persistence.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonLevelUpRepository(
    JsonStoragePaths paths,
    JsonFileReader reader,
    JsonFileWriter writer,
    JsonBackupService backupService,
    IOptions<JsonStorageOptions> options,
    ILogger<JsonLevelUpRepository> logger) : ILevelUpRepository, IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) =>
        ExecuteLockedAsync(LoadInternalAsync, cancellationToken);

    public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return ExecuteLockedAsync(token => SaveInternalAsync(data, token), cancellationToken);
    }

    public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mutation);

        return ExecuteLockedAsync(async token =>
        {
            var data = await LoadInternalAsync(token);
            mutation(data);
            data.EnsureValidState();
            await SaveInternalAsync(data, token);
        }, cancellationToken);
    }

    public void Dispose() => gate.Dispose();

    private async Task<LevelUpData> LoadInternalAsync(CancellationToken cancellationToken)
    {
        EnsureDirectories();

        if (!File.Exists(paths.DataFile))
        {
            var initialData = new LevelUpData();
            await SaveInternalAsync(initialData, cancellationToken);
            logger.LogInformation("Initial JSON persistence file created at {Path}.", paths.DataFile);
            return initialData;
        }

        try
        {
            var data = await reader.ReadAsync(paths.DataFile, cancellationToken);
            logger.LogDebug("JSON persistence file loaded from {Path}.", paths.DataFile);
            return data;
        }
        catch (DataFileCorruptedException originalException) when (options.Value.RecoverFromBackup)
        {
            logger.LogError(originalException, "Primary JSON file is invalid. Backup recovery will be attempted.");
            return await RestoreLatestBackupAsync(originalException, cancellationToken);
        }
    }

    private async Task<LevelUpData> RestoreLatestBackupAsync(
        Exception originalException,
        CancellationToken cancellationToken)
    {
        try
        {
            var recovered = await backupService.ReadLatestValidAsync(cancellationToken);
            var recoveryFile = paths.CreateRecoveryFile();

            try
            {
                await writer.WriteAsync(recoveryFile, recovered.Data, cancellationToken);
                File.Move(recoveryFile, paths.DataFile, overwrite: true);
            }
            finally
            {
                DeleteIfExists(recoveryFile);
            }

            logger.LogWarning(
                "Primary JSON persistence file restored from backup {BackupPath}.",
                recovered.Path);
            return recovered.Data;
        }
        catch (Exception backupException) when (backupException is PersistenceException or IOException)
        {
            throw new BackupRestoreException(
                "The primary JSON file is invalid and no backup could be restored.",
                new AggregateException(originalException, backupException));
        }
    }

    private async Task SaveInternalAsync(LevelUpData data, CancellationToken cancellationToken)
    {
        data.EnsureValidState();
        EnsureDirectories();

        var temporaryFile = paths.CreateTemporaryFile();
        try
        {
            await writer.WriteAsync(temporaryFile, data, cancellationToken);
            await reader.ReadAsync(temporaryFile, cancellationToken);
            await backupService.CreateAsync(cancellationToken);
            File.Move(temporaryFile, paths.DataFile, overwrite: true);

            logger.LogInformation("JSON persistence file saved atomically at {Path}.", paths.DataFile);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new PersistenceAccessException(paths.DataFile, exception);
        }
        finally
        {
            DeleteIfExists(temporaryFile);
        }
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(paths.StorageDirectory);
        Directory.CreateDirectory(paths.BackupDirectory);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private async Task ExecuteLockedAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            await operation(cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<T> ExecuteLockedAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await operation(cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }
}
