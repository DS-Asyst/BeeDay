using System.Text.Json;
using System.Text.Json.Nodes;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using LevelUp.Infrastructure.Diagnostics;
using LevelUp.Infrastructure.Persistence.Exceptions;
using Microsoft.Extensions.Logging;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonFileReader(
    JsonSerializerOptionsFactory serializerOptionsFactory,
    ILogger<JsonFileReader> logger)
{
    public async Task<LevelUpData> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16 * 1024,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            var node = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken)
                ?? throw new JsonException("The JSON file contains no LevelUp data.");

            LegacyActivityAttributeMigrator.Migrate(node);
            LegacyCharacterMigrator.Migrate(node);
            LegacyInventoryTagMigrator.Migrate(node);

            var data = node.Deserialize<LevelUpData>(serializerOptionsFactory.Create())
                ?? throw new JsonException("The JSON file contains no LevelUp data.");

            data.EnsureValidState();
            return data;
        }
        catch (Exception exception) when (exception is JsonException or DomainException or InvalidDataException)
        {
            logger.LogError(
                InfrastructureEventIds.DataFileInvalid,
                exception,
                "Invalid JSON persistence file detected. DataFilePath: {DataFilePath}",
                path);
            throw new DataFileCorruptedException(path, exception);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            throw new PersistenceAccessException(path, exception);
        }
    }
}
