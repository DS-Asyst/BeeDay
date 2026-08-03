namespace BeeDay.Infrastructure.Persistence.Exceptions;

public sealed class ConcurrencyConflictException : PersistenceException
{
    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
