using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BeeDay.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Lets `dotnet ef migrations` build a <see cref="BeeDayDbContext"/> without starting the full
/// BeeDay.Web host (production guard clauses, rate limiter, email sender, health checks). Used only by
/// EF Core tooling at design time — never resolved by the running application.
/// </summary>
internal sealed class BeeDayDbContextFactory : IDesignTimeDbContextFactory<BeeDayDbContext>
{
    // Placeholder credential only — the real beeday_dev password must never be committed. Set
    // BEEDAY_DESIGNTIME_CONNECTION locally (mirrors the User Secrets value used by BeeDay.Web) to
    // override this before running `dotnet ef` commands.
    private const string FallbackConnectionString =
        "Server=SERV4SQL;Database=BeeDay_Dev;User Id=beeday_dev;Password=CHANGEME;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=30";

    public BeeDayDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("BEEDAY_DESIGNTIME_CONNECTION")
            ?? FallbackConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<BeeDayDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new BeeDayDbContext(optionsBuilder.Options);
    }
}
