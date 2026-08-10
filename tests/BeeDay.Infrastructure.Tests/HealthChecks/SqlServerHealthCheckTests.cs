using BeeDay.Infrastructure.HealthChecks;
using BeeDay.Infrastructure.Persistence.SqlServer;
using BeeDay.Infrastructure.Tests.Persistence.SqlServer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace BeeDay.Infrastructure.Tests.HealthChecks;

[Collection("EfLocalDb")]
public sealed class SqlServerHealthCheckTests : EfLocalDbTestBase
{
    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsReachable_ReturnsHealthy()
    {
        var check = new SqlServerHealthCheck(ContextFactory);

        HealthCheckResult result = await check.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsUnreachable_ReturnsUnhealthy()
    {
        // A non-routable host fails fast (no DNS/route) rather than timing out slowly against a
        // real but unresponsive server - keeps this test quick without needing to stop any service.
        // CanConnectAsync handles a connection failure gracefully (returns false) rather than always
        // throwing, so this only asserts the Unhealthy status - not that an Exception is attached,
        // which only happens on the catch-block path, not this one.
        const string unreachableConnectionString =
            "Server=169.254.0.1;Database=BeeDayHealthCheckTests;Connect Timeout=1;Encrypt=False;TrustServerCertificate=True;";
        var services = new ServiceCollection();
        services.AddDbContextFactory<BeeDayDbContext>(options => options.UseSqlServer(unreachableConnectionString));
        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
        var check = new SqlServerHealthCheck(factory);

        HealthCheckResult result = await check.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
