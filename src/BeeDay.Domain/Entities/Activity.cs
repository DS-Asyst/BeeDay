using LevelUp.Domain.Abstractions;
using LevelUp.Domain.Common;
using LevelUp.Domain.Enums;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Domain.Entities;

public abstract class Activity : Entity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool Featured { get; private set; }

    public ActivityAttribute? Attribute { get; private set; }

    public virtual bool Completed { get; protected set; }

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public void AssignOwner(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }
        UserId = userId;
    }

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

    public void SetAttribute(ActivityAttribute? attribute)
    {
        Attribute = attribute.HasValue
            ? EnumValidation.Defined(attribute.Value, nameof(attribute))
            : null;
        Touch();
    }

    public virtual void ToggleCompletion()
    {
        Completed = !Completed;
        Touch();
    }

    protected void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
