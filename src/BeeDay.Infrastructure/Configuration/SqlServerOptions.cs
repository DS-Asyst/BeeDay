namespace LevelUp.Infrastructure.Configuration;

public sealed class SqlServerOptions
{
    public const string SectionName = "LevelUp:Persistence:SqlServer";

    public string ConnectionString { get; set; } = string.Empty;

    // Deliberately separate from ConnectionString being non-empty: a connection string may be present
    // only for local/design-time convenience (see LevelUpDbContextFactory) while JSON remains the only
    // active provider. The health check must stay off everywhere until something actually depends on
    // SQL Server being reachable — otherwise /health (which has no tag filter) starts failing the
    // instant a placeholder connection string exists, with no real database behind it.
    public bool HealthCheckEnabled { get; set; }
}
