namespace BeeDay.Domain.Events;

public interface IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOnUtc { get; }
}
