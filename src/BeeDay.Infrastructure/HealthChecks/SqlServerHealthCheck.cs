using BeeDay.Infrastructure.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BeeDay.Infrastructure.HealthChecks;

// Depends on IDbContextFactory<BeeDayDbContext>, not BeeDayDbContext directly — Blazor Server circuits
// are long-lived, so nothing in this codebase should hold a single scoped DbContext for the circuit's
// lifetime. Every operation, including this check, creates and disposes its own short-lived context.
internal sealed class SqlServerHealthCheck(IDbContextFactory<BeeDayDbContext> dbContextFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("SQL Server connection succeeded.")
                : HealthCheckResult.Unhealthy("SQL Server connection failed.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("SQL Server is unavailable.", exception);
        }
    }
}
