namespace BeeDay.Infrastructure.Persistence.Exceptions;

public sealed class PersistenceAccessException : PersistenceException
{
    public PersistenceAccessException(string path, Exception innerException)
        : base($"The persistence path '{path}' could not be accessed.", innerException)
    {
        Path = path;
    }

    public string Path { get; }
}
