namespace LevelUp.Infrastructure.Persistence.Json;

public sealed class JsonStorageInitializer(JsonStoragePaths paths)
{
    public void EnsureCreated()
    {
        Directory.CreateDirectory(paths.StorageDirectory);
        Directory.CreateDirectory(paths.BackupDirectory);
    }
}
