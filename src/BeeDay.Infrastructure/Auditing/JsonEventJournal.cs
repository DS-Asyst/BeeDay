using System.Text.Json;
using System.Text.Json.Serialization;
using BeeDay.Application.Common.Auditing;
using BeeDay.Domain.Events;
using BeeDay.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.Auditing;

/// <summary>
/// Append-only audit log for domain events — entirely independent of application-state persistence.
/// <see cref="IEventJournal"/> is write-only (no read-back API), its only consumer is
/// <c>AuditDomainEventHandler</c> (fire-and-forget via the background task queue), and it writes to its
/// own file, never read by anything else. Before Sprint 14.7 this depended on the JSON document-store
/// path/serializer helpers (<c>JsonStoragePaths</c>, <c>JsonSerializerOptionsFactory</c>) purely by
/// convenience of file co-location — genuinely unrelated dependencies, since domain events are plain
/// immutable records with public <c>init</c> properties (<see cref="DomainEvent"/> and its derivatives)
/// and serialize correctly with a plain <see cref="JsonSerializerOptions"/>; they never needed the
/// custom <c>DomainJsonContractResolver</c> built specifically for <c>LevelUpData</c>/entity private
/// setters. This class now resolves its own file location from <see cref="EventJournalOptions"/> alone.
/// </summary>
public sealed class JsonEventJournal : IEventJournal
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string journalPath;
    private readonly SemaphoreSlim writeLock = new(1, 1);

    public JsonEventJournal(IHostEnvironment environment, IOptions<EventJournalOptions> options)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);

        var root = Path.GetFullPath(environment.ContentRootPath);
        var directory = Path.IsPathRooted(options.Value.Directory)
            ? Path.GetFullPath(options.Value.Directory)
            : Path.GetFullPath(Path.Combine(root, options.Value.Directory));
        journalPath = Path.Combine(directory, options.Value.FileName);
    }

    public async Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var directory = Path.GetDirectoryName(journalPath)
            ?? throw new InvalidOperationException("The event journal directory could not be resolved.");
        Directory.CreateDirectory(directory);

        await writeLock.WaitAsync(cancellationToken);
        try
        {
            if (await ContainsAsync(domainEvent, cancellationToken))
            {
                return;
            }

            object envelope = CreateEnvelope(domainEvent);
            string json = JsonSerializer.Serialize(envelope, SerializerOptions);
            await File.AppendAllTextAsync(journalPath, json + Environment.NewLine, cancellationToken);
        }
        finally
        {
            writeLock.Release();
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

    private async Task<bool> ContainsAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
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
