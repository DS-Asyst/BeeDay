namespace LevelUp.Infrastructure.Persistence.Entities;

public sealed class GameMetadataEntity
{
    public int Id { get; set; } = 1;
    public int SchemaVersion { get; set; }
    public int SaveRevision { get; set; }
    public DateTime? LastSavedAt { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
