using System.Text.Json;
using LevelUp.Domain.Entities;
using LevelUp.Infrastructure.Persistence.Exceptions;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonFileWriter(JsonSerializerOptionsFactory serializerOptionsFactory)
{
    public async Task WriteAsync(
        string path,
        LevelUpData data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        data.EnsureValidState();

        try
        {
            await using var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16 * 1024,
                options: FileOptions.Asynchronous | FileOptions.WriteThrough);

            await JsonSerializer.SerializeAsync(
                stream,
                data,
                serializerOptionsFactory.Create(),
                cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or JsonException)
        {
            throw new PersistenceAccessException(path, exception);
        }
    }
}
