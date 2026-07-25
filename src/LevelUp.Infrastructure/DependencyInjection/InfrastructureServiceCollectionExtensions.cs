using LevelUp.Application.Common.Contracts;
using LevelUp.Infrastructure.Auditing;
using LevelUp.Infrastructure.Background;
using LevelUp.Infrastructure.Caching;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.HealthChecks;
using LevelUp.Infrastructure.Identity;
using LevelUp.Infrastructure.Persistence.Json;
using LevelUp.Infrastructure.Security;
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
            .AddOptions<IdentityEmailOptions>()
            .Bind(configuration.GetSection(IdentityEmailOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out _), "Identity email public base URL must be absolute.")
            .Validate(options => IsRootedRelativePath(options.ConfirmationPath), "Email confirmation path must start with '/'.")
            .Validate(options => IsRootedRelativePath(options.PasswordResetPath), "Password reset path must start with '/'.")
            .ValidateOnStart();

        services
            .AddOptions<DevelopmentEmailOptions>()
            .Bind(configuration.GetSection(DevelopmentEmailOptions.SectionName))
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Directory), "Development email directory is required when capture is enabled.")
            .ValidateOnStart();

        services
            .AddOptions<ResendOptions>()
            .Bind(configuration.GetSection(ResendOptions.SectionName))
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ApiKey), "Resend API key is required when email delivery is enabled.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.FromAddress), "Resend sender address is required when email delivery is enabled.")
            .Validate(options => !options.Enabled || options.FromAddress.Contains('@', StringComparison.Ordinal), "Resend sender address must be a valid email address.")
            .ValidateOnStart();

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
        services.AddSingleton<LevelUp.Application.Common.Security.IPasswordService, Pbkdf2PasswordService>();
        services.AddSingleton<LevelUp.Application.Common.Identity.IClock, SystemClock>();
        services.AddSingleton<LevelUp.Application.Common.Identity.IUserTokenService, SecureUserTokenService>();
        services.AddSingleton<LevelUp.Application.Common.Identity.IIdentityEmailComposer, IdentityEmailComposer>();
        var resendEnabled = configuration.GetValue<bool>($"{ResendOptions.SectionName}:Enabled");
        if (resendEnabled)
        {
            services.AddHttpClient<LevelUp.Application.Common.Identity.IEmailSender, ResendEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        }
        else
        {
            services.AddSingleton<LevelUp.Application.Common.Identity.IEmailSender, DevelopmentEmailSender>();
        }
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

    private static bool IsRootedRelativePath(string? path) =>
        !string.IsNullOrWhiteSpace(path)
        && path.StartsWith("/", StringComparison.Ordinal)
        && !path.StartsWith("//", StringComparison.Ordinal);

    private static bool IsSimpleDirectoryName(string? name) =>
        !string.IsNullOrWhiteSpace(name)
        && string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal)
        && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
}
