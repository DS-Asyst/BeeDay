using System.Diagnostics;
using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.Diagnostics;
using LevelUp.Infrastructure.Persistence.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonLevelUpRepository(
    JsonStoragePaths paths,
    JsonFileReader reader,
    JsonFileWriter writer,
    JsonBackupService backupService,
    JsonStorageGate storageGate,
    JsonStorageInitializer storageInitializer,
    JsonAtomicFileCommitter fileCommitter,
    IOptions<JsonStorageOptions> options,
    ILogger<JsonLevelUpRepository> logger) : ILevelUpRepository
{

    public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) =>
        storageGate.ExecuteAsync(LoadInternalAsync, cancellationToken);

    public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        return storageGate.ExecuteAsync(token => SaveInternalAsync(data, token), cancellationToken);
    }

    public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mutation);

        return storageGate.ExecuteAsync(async token =>
        {
            var data = await LoadInternalAsync(token);
            mutation(data);
            data.EnsureValidState();
            await SaveInternalAsync(data, token);
        }, cancellationToken);
    }


    private async Task<LevelUpData> LoadInternalAsync(CancellationToken cancellationToken)
    {
        storageInitializer.EnsureCreated();

        if (!File.Exists(paths.DataFile))
        {
            var initialData = new LevelUpData();
            await SaveInternalAsync(initialData, cancellationToken);
            logger.LogInformation(
                InfrastructureEventIds.DataFileCreated,
                "Initial JSON persistence file created. DataFilePath: {DataFilePath}",
                paths.DataFile);
            return initialData;
        }

        try
        {
            var data = await reader.ReadAsync(paths.DataFile, cancellationToken);
            logger.LogDebug(
                InfrastructureEventIds.DataFileLoaded,
                "JSON persistence file loaded. DataFilePath: {DataFilePath}",
                paths.DataFile);
            return data;
        }
        catch (DataFileCorruptedException originalException) when (options.Value.RecoverFromBackup)
        {
            logger.LogError(
                InfrastructureEventIds.DataFileInvalid,
                originalException,
                "Primary JSON file is invalid. Backup recovery will be attempted. DataFilePath: {DataFilePath}",
                paths.DataFile);
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
                fileCommitter.Commit(recoveryFile, paths.DataFile);
            }
            finally
            {
                fileCommitter.DeleteTemporaryFile(recoveryFile);
            }

            logger.LogWarning(
                InfrastructureEventIds.BackupRestored,
                "Primary JSON persistence file restored from backup. BackupPath: {BackupPath}, DataFilePath: {DataFilePath}",
                recovered.Path,
                paths.DataFile);
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
        storageInitializer.EnsureCreated();

        var temporaryFile = paths.CreateTemporaryFile();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await writer.WriteAsync(temporaryFile, data, cancellationToken);
            await reader.ReadAsync(temporaryFile, cancellationToken);
            await backupService.CreateAsync(cancellationToken);
            fileCommitter.Commit(temporaryFile, paths.DataFile);

            stopwatch.Stop();
            logger.LogInformation(
                InfrastructureEventIds.DataFileSaved,
                "JSON persistence file saved atomically. DataFilePath: {DataFilePath}, DurationMs: {DurationMs}",
                paths.DataFile,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new PersistenceAccessException(paths.DataFile, exception);
        }
        finally
        {
            fileCommitter.DeleteTemporaryFile(temporaryFile);
        }
    }
}
