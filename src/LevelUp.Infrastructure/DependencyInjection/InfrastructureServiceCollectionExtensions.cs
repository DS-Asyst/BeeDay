using LevelUp.Application.Common.Contracts;
using LevelUp.Infrastructure.Auditing;
using LevelUp.Infrastructure.Background;
using LevelUp.Infrastructure.Caching;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.HealthChecks;
using LevelUp.Infrastructure.Persistence.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LevelUp.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddLevelUpInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<JsonStorageOptions>()
            .Bind(configuration.GetSection(JsonStorageOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Directory), "Storage directory is required.")
            .Validate(options => IsJsonFile(options.FileName), "Storage file name must be a JSON file.")
            .Validate(options => IsSimpleDirectoryName(options.BackupDirectory), "Backup directory must be a simple relative directory name.")
            .Validate(options => options.BackupRetention is >= 1 and <= 100, "Backup retention must be between 1 and 100.")
            .ValidateOnStart();

        services.AddSingleton<JsonStoragePaths>();
        services.AddSingleton<JsonSerializerOptionsFactory>();
        services.AddSingleton<JsonFileReader>();
        services.AddSingleton<JsonFileWriter>();
        services.AddSingleton<JsonBackupService>();
        services.AddSingleton<ILevelUpRepository, JsonLevelUpRepository>();
        services.AddMemoryCache();
        services.AddSingleton<MemoryApplicationCache>();
        services.AddSingleton<LevelUp.Application.Common.Caching.IApplicationCache>(sp => sp.GetRequiredService<MemoryApplicationCache>());
        services.AddSingleton<JsonEventJournal>();
        services.AddSingleton<LevelUp.Application.Common.Auditing.IEventJournal>(sp => sp.GetRequiredService<JsonEventJournal>());
        services.AddSingleton<BackgroundTaskQueue>();
        services.AddSingleton<LevelUp.Application.Common.Background.IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
        services.AddHostedService<BackgroundTaskWorker>();
        services.AddHealthChecks()
            .AddCheck<JsonStorageHealthCheck>(
                "json-storage",
                tags: ["ready", "storage"]);

        return services;
    }

    private static bool IsJsonFile(string? name) =>
        !string.IsNullOrWhiteSpace(name)
        && string.Equals(Path.GetExtension(name), ".json", StringComparison.OrdinalIgnoreCase)
        && string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal);

    private static bool IsSimpleDirectoryName(string? name) =>
        !string.IsNullOrWhiteSpace(name)
        && string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal)
        && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
}
