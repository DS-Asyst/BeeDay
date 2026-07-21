using System.Text.Json;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
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

            var data = await JsonSerializer.DeserializeAsync<LevelUpData>(
                stream,
                serializerOptionsFactory.Create(),
                cancellationToken) ?? throw new JsonException("The JSON file contains no LevelUp data.");

            data.EnsureValidState();
            return data;
        }
        catch (Exception exception) when (exception is JsonException or DomainException or InvalidDataException)
        {
            logger.LogError(exception, "Invalid JSON persistence file detected at {Path}.", path);
            throw new DataFileCorruptedException(path, exception);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            throw new PersistenceAccessException(path, exception);
        }
    }
}
