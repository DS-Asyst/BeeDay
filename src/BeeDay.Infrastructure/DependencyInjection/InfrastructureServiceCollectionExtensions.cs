using BeeDay.Application.Common.Contracts;
using BeeDay.Infrastructure.Auditing;
using BeeDay.Infrastructure.Background;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.HealthChecks;
using BeeDay.Infrastructure.Identity;
using BeeDay.Infrastructure.Persistence.SqlServer;
using BeeDay.Infrastructure.Persistence.SqlServer.Repositories;
using BeeDay.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddBeeDayInfrastructure(
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
            .AddOptions<SqlServerOptions>()
            .Bind(configuration.GetSection(SqlServerOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "SQL Server connection string is required.")
            .ValidateOnStart();

        services
            .AddOptions<EventJournalOptions>()
            .Bind(configuration.GetSection(EventJournalOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Directory), "Event journal directory is required.")
            .Validate(options => IsSimpleDirectoryName(options.FileName), "Event journal file name must be a simple file name.")
            .ValidateOnStart();

        // JsonEventJournal (Sprint 14.7): domain-event audit log, entirely independent of application-
        // state persistence — see the class's own XML doc. No JSON persistence provider is registered
        // anywhere in this method; JsonEventJournal is the only "Json"-named type left, and it never
        // reads or writes functional state.
        services.AddSingleton<JsonEventJournal>();
        services.AddSingleton<BeeDay.Application.Common.Auditing.IEventJournal>(sp => sp.GetRequiredService<JsonEventJournal>());
        services.AddScoped<BeeDay.Application.Features.Wallets.Contracts.IWalletReadService, EfWalletReadService>();
        services.AddScoped<BeeDay.Application.Features.Dashboard.Contracts.IDashboardReadService, EfDashboardReadService>();
        services.AddSingleton<BeeDay.Application.Common.Security.IPasswordService, Pbkdf2PasswordService>();
        services.AddSingleton<BeeDay.Application.Common.Identity.IClock, SystemClock>();
        services.AddSingleton<BeeDay.Application.Common.Identity.IUserTokenService, SecureUserTokenService>();
        services.AddSingleton<BeeDay.Application.Common.Identity.IIdentityRequestThrottle, MemoryIdentityRequestThrottle>();
        services.AddSingleton<BeeDay.Application.Common.Identity.IIdentityEmailComposer, IdentityEmailComposer>();
        // Falls back to each Options class's own declared default (not bool's default false) when the
        // key is absent, matching exactly what .Bind() would have produced — an omitted key must
        // resolve the same way here as it does for every other consumer of these Options classes.
        var resendEnabled = configuration.GetValue($"{ResendOptions.SectionName}:Enabled", new ResendOptions().Enabled);
        var developmentEmailEnabled = configuration.GetValue($"{DevelopmentEmailOptions.SectionName}:Enabled", new DevelopmentEmailOptions().Enabled);
        var emailProvider = EmailProviderSelector.Resolve(resendEnabled, developmentEmailEnabled);
        if (emailProvider == EmailProvider.Resend)
        {
            // HmgRecipientGuardOptions is only registered/validated here — never unconditionally for
            // every environment — because it exists solely to guard Resend delivery. Registering its
            // ValidateOnStart outside this branch would fail startup for the Development provider too,
            // which never consults it. See HmgRecipientGuardedEmailSender and
            // docs/infrastructure/06-transactional-email.md §10 for the fail-closed contract this
            // enforces: Enabled defaults to true, so an environment that switches to Resend without ever
            // configuring this section refuses to start rather than sending unprotected.
            services
                .AddOptions<HmgRecipientGuardOptions>()
                .Bind(configuration.GetSection(HmgRecipientGuardOptions.SectionName))
                .Validate(options => !options.Enabled || options.AllowedRecipients.Count > 0, "At least one allowed recipient is required while the HMG recipient guard is enabled.")
                .ValidateOnStart();

            services.AddHttpClient<ResendEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddSingleton<BeeDay.Application.Common.Identity.IEmailSender, HmgRecipientGuardedEmailSender>(sp =>
                new HmgRecipientGuardedEmailSender(
                    sp.GetRequiredService<ResendEmailSender>(),
                    sp.GetRequiredService<IOptions<HmgRecipientGuardOptions>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<HmgRecipientGuardedEmailSender>>()));
        }
        else
        {
            services.AddSingleton<BeeDay.Application.Common.Identity.IEmailSender, DevelopmentEmailSender>();
        }
        services.AddSingleton<BackgroundTaskQueue>();
        services.AddSingleton<BeeDay.Application.Common.Background.IBackgroundTaskQueue>(sp => sp.GetRequiredService<BackgroundTaskQueue>());
        services.AddHostedService<BackgroundTaskWorker>();

        // AddDbContextFactory, not AddDbContext: BeeDay.Web is Blazor Server, whose SignalR circuits
        // are long-lived. A DbContext registered as scoped would live for the whole circuit — unsafe
        // for a type that is not thread-safe and not meant to track state across many operations. Every
        // future adapter must resolve IDbContextFactory<BeeDayDbContext> and create/dispose a
        // short-lived context per operation via CreateDbContext()/CreateDbContextAsync().
        services.AddDbContextFactory<BeeDayDbContext>((serviceProvider, options) =>
        {
            var sqlServerOptions = serviceProvider.GetRequiredService<IOptions<SqlServerOptions>>().Value;
            options.UseSqlServer(sqlServerOptions.ConnectionString);
        });

        // The 8 per-Aggregate persistence contracts defined in EPIC 13
        // (docs/history/persistence-contracts.md), adapted against SQL Server (EPIC 14). Every
        // production handler depends on one of these (or IUnitOfWork below) as of Sprint 14.6 — SQL
        // Server is the only active runtime provider.
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserTokenRepository, EfUserTokenRepository>();
        services.AddScoped<IHabitRepository, EfHabitRepository>();
        services.AddScoped<IRecurringTaskRepository, EfRecurringTaskRepository>();
        services.AddScoped<IProjectRepository, EfProjectRepository>();
        services.AddScoped<IWalletRepository, EfWalletRepository>();
        services.AddScoped<IWalletTagRepository, EfWalletTagRepository>();
        services.AddScoped<ITransactionRepository, EfTransactionRepository>();

        // Sprint 14.5: coordinates the 8 repositories above against one shared BeeDayDbContext, for
        // callers that need multiple writes to commit/roll back together. AddTransient, not AddScoped —
        // EfUnitOfWork eagerly creates and holds a context for its own lifetime; a Scoped registration
        // would let it live for the whole Blazor Server circuit (the exact problem AddDbContextFactory
        // exists to avoid). Every resolution creates a fresh instance and a fresh context; the resolver
        // is responsible for disposing it (await using) once its unit of work is done.
        services.AddTransient<IUnitOfWork, EfUnitOfWork>();

        // SQL Server is the only active runtime provider as of Sprint 14.6 — this check now takes over
        // the "ready"/"storage" tags JsonStorageHealthCheck used to own (that check had no consumer left
        // once the JSON document store was de-registered above, so it was removed rather than kept
        // dormant). Runs unconditionally in every environment, matching that the connection string
        // itself is already required (see the Validate call above), not optional/local-only.
        services.AddHealthChecks()
            .AddCheck<SqlServerHealthCheck>("sql-server", tags: ["ready", "storage", "sql"]);

        return services;
    }

    private static bool IsRootedRelativePath(string? path) =>
        !string.IsNullOrWhiteSpace(path)
        && path.StartsWith("/", StringComparison.Ordinal)
        && !path.StartsWith("//", StringComparison.Ordinal);

    private static bool IsSimpleDirectoryName(string? name) =>
        !string.IsNullOrWhiteSpace(name)
        && string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal)
        && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
}
