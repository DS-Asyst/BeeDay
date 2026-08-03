namespace BeeDay.Infrastructure.Persistence.Exceptions;

public sealed class DataFileCorruptedException : PersistenceException
{
    public DataFileCorruptedException(string path, Exception innerException)
        : base($"The JSON data file '{path}' is corrupted or contains an invalid domain state.", innerException)
    {
        Path = path;
    }

    public string Path { get; }
}
