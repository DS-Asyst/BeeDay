using System.Text.Json;
using LevelUp.Application.Common.Auditing;
using LevelUp.Domain.Events;
using LevelUp.Infrastructure.Persistence.Json;

namespace LevelUp.Infrastructure.Auditing;

public sealed class JsonEventJournal(
    JsonStoragePaths storagePaths,
    JsonSerializerOptionsFactory serializerOptionsFactory) : IEventJournal
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public async Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var directory = Path.GetDirectoryName(storagePaths.DataFile)
            ?? throw new InvalidOperationException("The storage directory could not be resolved.");
        Directory.CreateDirectory(directory);

        var journalPath = Path.Combine(directory, "LevelUpEvents.ndjson");
        var envelope = new
        {
            Type = domainEvent.GetType().Name,
            domainEvent.EventId,
            domainEvent.OccurredOnUtc,
            Payload = (object)domainEvent
        };

        var json = JsonSerializer.Serialize(envelope, serializerOptionsFactory.Create());

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(journalPath, json + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
