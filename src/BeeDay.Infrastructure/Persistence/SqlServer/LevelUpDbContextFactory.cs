using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BeeDay.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Lets `dotnet ef migrations` build a <see cref="LevelUpDbContext"/> without starting the full
/// BeeDay.Web host (production guard clauses, rate limiter, email sender, health checks). Used only by
/// EF Core tooling at design time — never resolved by the running application.
/// </summary>
internal sealed class LevelUpDbContextFactory : IDesignTimeDbContextFactory<LevelUpDbContext>
{
    private const string FallbackConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=LevelUpDev;Trusted_Connection=True;TrustServerCertificate=True;";

    public LevelUpDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("LEVELUP_DESIGNTIME_CONNECTION")
            ?? FallbackConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<LevelUpDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new LevelUpDbContext(optionsBuilder.Options);
    }
}
