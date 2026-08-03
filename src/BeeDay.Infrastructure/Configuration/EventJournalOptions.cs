namespace BeeDay.Infrastructure.Configuration;

/// <summary>
/// Configuration for <see cref="Auditing.JsonEventJournal"/>'s own append-only audit log — deliberately
/// separate from the (removed, Sprint 14.7) JSON persistence options. The event journal never
/// participated in functional-state persistence; it only ever needed a directory and a file name.
/// </summary>
public sealed class EventJournalOptions
{
    public const string SectionName = "LevelUp:Auditing:EventJournal";

    public string Directory { get; set; } = "Data";
    public string FileName { get; set; } = "LevelUpEvents.ndjson";
}
