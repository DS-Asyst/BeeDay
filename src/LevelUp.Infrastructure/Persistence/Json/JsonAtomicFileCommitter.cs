using LevelUp.Infrastructure.Persistence.Exceptions;

namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonAtomicFileCommitter
{
    public void Commit(string temporaryFile, string destinationFile)
    {
        try
        {
            File.Move(temporaryFile, destinationFile, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new PersistenceAccessException(destinationFile, exception);
        }
    }

    public void DeleteTemporaryFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new PersistenceAccessException(path, exception);
        }
    }
}
