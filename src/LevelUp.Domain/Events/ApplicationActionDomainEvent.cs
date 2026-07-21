namespace LevelUp.Domain.Events;

public sealed record ApplicationActionDomainEvent(
    string Action,
    string Category,
    string? EntityId = null) : DomainEvent;
