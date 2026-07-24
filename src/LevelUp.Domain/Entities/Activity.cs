using System.Text.Json.Serialization;
using LevelUp.Domain.Abstractions;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Domain.Entities;

public abstract class Activity : Entity
{
    [JsonInclude]
    public string Title { get; private set; } = string.Empty;

    [JsonInclude]
    public string Description { get; private set; } = string.Empty;

    [JsonInclude]
    public bool Featured { get; private set; }

    [JsonInclude]
    public virtual bool Completed { get; protected set; }

    [JsonInclude]
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    [JsonInclude]
    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    protected void UpdateDetails(string title, string? description)
    {
        Title = ActivityTitle.Create(title).Value;
        Description = ActivityDescription.Create(description).Value;
        Touch();
    }

    public void SetFeatured(bool featured)
    {
        Featured = featured;
        Touch();
    }

    public virtual void ToggleCompletion()
    {
        Completed = !Completed;
        Touch();
    }

    protected void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
