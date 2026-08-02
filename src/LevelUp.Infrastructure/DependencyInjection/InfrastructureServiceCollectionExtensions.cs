using LevelUp.Application.Common.Contracts;
using LevelUp.Infrastructure.Auditing;
using LevelUp.Infrastructure.Background;
using LevelUp.Infrastructure.Caching;
using LevelUp.Infrastructure.Configuration;
using LevelUp.Infrastructure.HealthChecks;
using LevelUp.Infrastructure.Identity;
using LevelUp.Infrastructure.Persistence.Json;
using LevelUp.Infrastructure.Persistence.SqlServer;
using LevelUp.Infrastructure.Persistence.SqlServer.Repositories;
using LevelUp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services
            .AddOptions<SqlServerOptions>()
            .Bind(configuration.GetSection(SqlServerOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<JsonStoragePaths>();
        services.AddSingleton<JsonStorageGate>();
        services.AddSingleton<JsonStorageInitializer>();
        services.AddSingleton<JsonAtomicFileCommitter>();
        services.AddSingleton<JsonSerializerOptionsFactory>();
        services.AddSingleton<JsonFileReader>();
        services.AddSingleton<JsonFileWriter>();
        services.AddSingleton<JsonBackupService>();
        services.AddSingleton<JsonLevelUpDocumentStore>();
        services.AddSingleton<ILevelUpRepository, JsonLevelUpRepository>();
        services.AddSingleton<LevelUp.Application.Features.Wallets.Contracts.IWalletReadService, JsonWalletReadService>();
        services.AddSingleton<LevelUp.Application.Features.Dashboard.Contracts.IDashboardReadService, JsonDashboardReadService>();
        services.AddSingleton<LevelUp.Application.Common.Security.IPasswordService, Pbkdf2PasswordService>();
        services.AddSingleton<LevelUp.Application.Common.Identity.IClock, SystemClock>();
        services.AddSingleton<LevelUp.Application.Common.Identity.IUserTokenService, SecureUserTokenService>();
        services.AddSingleton<LevelUp.Application.Common.Identity.IIdentityRequestThrottle, MemoryIdentityRequestThrottle>();
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

        // AddDbContextFactory, not AddDbContext: LevelUp.Web is Blazor Server, whose SignalR circuits
        // are long-lived. A DbContext registered as scoped would live for the whole circuit — unsafe
        // for a type that is not thread-safe and not meant to track state across many operations. Every
        // future adapter must resolve IDbContextFactory<LevelUpDbContext> and create/dispose a
        // short-lived context per operation via CreateDbContext()/CreateDbContextAsync().
        services.AddDbContextFactory<LevelUpDbContext>((serviceProvider, options) =>
        {
            var sqlServerOptions = serviceProvider.GetRequiredService<IOptions<SqlServerOptions>>().Value;
            options.UseSqlServer(sqlServerOptions.ConnectionString);
        });

        // Sprint 14.4: the first real adapters for the 8 per-Aggregate persistence contracts defined in
        // EPIC 13 (docs/architecture/07-persistence-contracts.md). Registering them makes the contracts
        // *resolvable*, not *consumed* — no handler depends on any of these interfaces yet (they all
        // still depend on ILevelUpRepository/JSON, unchanged by this Sprint), so this does not make SQL
        // Server the active provider.
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserTokenRepository, EfUserTokenRepository>();
        services.AddScoped<IHabitRepository, EfHabitRepository>();
        services.AddScoped<IRecurringTaskRepository, EfRecurringTaskRepository>();
        services.AddScoped<IProjectRepository, EfProjectRepository>();
        services.AddScoped<IWalletRepository, EfWalletRepository>();
        services.AddScoped<IWalletTagRepository, EfWalletTagRepository>();
        services.AddScoped<ITransactionRepository, EfTransactionRepository>();

        services.AddHealthChecks()
            .AddCheck<JsonStorageHealthCheck>(
                "json-storage",
                tags: ["ready", "storage"]);

        // Registered only when explicitly enabled (default off — see SqlServerOptions.HealthCheckEnabled),
        // and tagged "sql" only (never "ready"/"storage") — JSON remains the only active provider, and
        // /health has no tag filter, so this must stay dormant until something actually depends on SQL
        // Server being reachable.
        var sqlServerHealthCheckEnabled = configuration.GetValue<bool>(
            $"{SqlServerOptions.SectionName}:HealthCheckEnabled");
        if (sqlServerHealthCheckEnabled)
        {
            services.AddHealthChecks()
                .AddCheck<SqlServerHealthCheck>("sql-server", tags: ["sql"]);
        }

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
