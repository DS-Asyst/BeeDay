namespace BeeDay.Infrastructure.Configuration;

public sealed class SqlServerOptions
{
    public const string SectionName = "BeeDay:Persistence:SqlServer";

    public string ConnectionString { get; set; } = string.Empty;
}
