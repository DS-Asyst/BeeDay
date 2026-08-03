namespace BeeDay.Application.Exceptions;

public sealed class ActivityNotFoundException(Guid id) : KeyNotFoundException($"Activity '{id}' was not found.")
{
    public Guid ActivityId { get; } = id;
}
