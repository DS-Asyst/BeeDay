using System.Text.Json;
using LevelUp.Application.Common.Auditing;
using LevelUp.Domain.Events;
using LevelUp.Infrastructure.Persistence.Json;

namespace LevelUp.Infrastructure.Auditing;

public sealed class JsonEventJournal(
    JsonStoragePaths storagePaths,
    JsonSerializerOptionsFactory serializerOptionsFactory) : IEventJournal
{
    private const string JournalFileName = "LevelUpEvents.ndjson";
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public async Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        string directory = Path.GetDirectoryName(storagePaths.DataFile)
            ?? throw new InvalidOperationException("The storage directory could not be resolved.");
        Directory.CreateDirectory(directory);

        string journalPath = Path.Combine(directory, JournalFileName);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            if (await ContainsAsync(journalPath, domainEvent, cancellationToken))
            {
                return;
            }

            object envelope = CreateEnvelope(domainEvent);
            string json = JsonSerializer.Serialize(envelope, serializerOptionsFactory.Create());
            await File.AppendAllTextAsync(journalPath, json + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static object CreateEnvelope(IDomainEvent domainEvent)
    {
        string? summary = domainEvent switch
        {
            UserLeveledUpDomainEvent leveledUpEvent =>
                $"Reached level {leveledUpEvent.NewLevel} after gaining {leveledUpEvent.ExperienceAmount} XP from {leveledUpEvent.ExperienceSource}.",
            _ => null,
        };

        return new
        {
            Type = domainEvent.GetType().Name,
            domainEvent.EventId,
            domainEvent.OccurredOnUtc,
            Summary = summary,
            Payload = (object)domainEvent,
        };
    }

    private static async Task<bool> ContainsAsync(
        string journalPath,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(journalPath))
        {
            return false;
        }

        await using var stream = new FileStream(
            journalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize: 4096,
            useAsync: true);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("eventId", out JsonElement eventIdElement) &&
                    eventIdElement.TryGetGuid(out Guid eventId) &&
                    eventId == domainEvent.EventId)
                {
                    return true;
                }

                if (domainEvent is UserLeveledUpDomainEvent leveledUpEvent &&
                    IsSameLevelUpEntry(root, leveledUpEvent.ExperienceEntryId))
                {
                    return true;
                }
            }
            catch (JsonException)
            {
                // Preserve journal availability when an older malformed line is encountered.
            }
        }

        return false;
    }

    private static bool IsSameLevelUpEntry(JsonElement root, Guid experienceEntryId)
    {
        if (!root.TryGetProperty("type", out JsonElement typeElement) ||
            !string.Equals(
                typeElement.GetString(),
                nameof(UserLeveledUpDomainEvent),
                StringComparison.Ordinal))
        {
            return false;
        }

        return root.TryGetProperty("payload", out JsonElement payloadElement) &&
            payloadElement.TryGetProperty("experienceEntryId", out JsonElement entryIdElement) &&
            entryIdElement.TryGetGuid(out Guid storedEntryId) &&
            storedEntryId == experienceEntryId;
    }
}
