using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LevelUp.Infrastructure.HealthChecks;

public sealed class JsonStorageHealthCheck(
    JsonStoragePaths paths,
    JsonFileReader reader) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(paths.StorageDirectory);
            Directory.CreateDirectory(paths.BackupDirectory);

            var probe = Path.Combine(paths.StorageDirectory, $".health-{Guid.NewGuid():N}.tmp");
            try
            {
                await File.WriteAllTextAsync(probe, "ok", cancellationToken);
            }
            finally
            {
                if (File.Exists(probe))
                {
                    File.Delete(probe);
                }
            }

            if (File.Exists(paths.DataFile))
            {
                await reader.ReadAsync(paths.DataFile, cancellationToken);
            }

            return HealthCheckResult.Healthy(
                "JSON persistence is readable, writable and structurally valid.",
                new Dictionary<string, object>
                {
                    ["dataFile"] = paths.DataFile,
                    ["backupDirectory"] = paths.BackupDirectory
                });
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "JSON persistence is unavailable, read-only or invalid.",
                exception,
                new Dictionary<string, object>
                {
                    ["dataFile"] = paths.DataFile
                });
        }
    }
}
