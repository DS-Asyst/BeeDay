using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Security;
using BeeDay.Domain.Entities;
using BeeDay.Infrastructure.Persistence.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Hosts the real LevelUp application on a real Kestrel TCP endpoint (not TestServer's in-memory
/// transport) so a genuine Chromium instance can navigate to it. Self-contained: does not inherit
/// from or reference any type in BeeDay.Web.Tests — only the production BeeDay.Web project.
/// Storage is an isolated, disposable SQL Server LocalDB database per instance (migrated on startup,
/// dropped on Dispose) — the Sprint 14.6 SQL Server cutover replaced the JSON temp-directory isolation
/// this class used before, re-implemented here in miniature since sharing the actual Web.Tests factory
/// class would require a test-to-test reference.
/// </summary>
public sealed class E2EWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=LevelUp_E2ETests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True;";

    public E2EWebApplicationFactory() => UseKestrel(port: 0);

    /// <summary>
    /// The real, OS-assigned address Kestrel bound to, populated by the base class once the server
    /// has actually started (triggered by touching <see cref="WebApplicationFactory{TEntryPoint}.Server"/>).
    /// <c>UseKestrel(int)</c> — the first-party API .NET 10's Microsoft.AspNetCore.Mvc.Testing
    /// added for exactly this scenario — makes this synchronous and deterministic: by the time the
    /// constructor call above returns control to a caller that then reads this property (after
    /// touching Server), Kestrel is already bound. No polling, no fixed delay, and since this runs
    /// in-process there is no child OS process to ever leak.
    /// </summary>
    public string ServerAddress => ClientOptions.BaseAddress.ToString().TrimEnd('/');

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LevelUp:Persistence:SqlServer:ConnectionString"] = connectionString,
            ["LevelUp:Email:Development:Enabled"] = "false",
            // Generous limits: E2E exercises real user journeys, not the rate limiter itself
            // (already covered by Sprint 12.6's integration tests) — this just keeps it out of the way.
            ["LevelUp:RateLimiting:Login:IpPermitLimit"] = "1000",
            ["LevelUp:RateLimiting:Login:EmailPermitLimit"] = "1000",
            ["LevelUp:RateLimiting:Login:Window"] = "00:00:01",
            ["LevelUp:RateLimiting:Login:SegmentsPerWindow"] = "1"
        }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LevelUpDbContext>>();
        using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();

        return host;
    }

    /// <summary>
    /// Creates a confirmed, active User directly through the repository (bypassing HTTP/UI), with
    /// onboarding optionally already completed. Every E2E test that needs a signed-in starting point
    /// uses this for arrange-only setup; the actual behavior under test is always driven through the
    /// real browser afterward.
    /// </summary>
    public async Task<User> SeedUserAsync(string email, string password, bool onboardingCompleted)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var nickname = $"e2e{Guid.NewGuid():N}"[..12];

        var user = User.Create("E2E Test User", email, passwordService.Hash(password));
        user.ConfirmEmail(user.CreatedAtUtc);
        user.CompleteProfile(nickname, avatar: null);
        if (onboardingCompleted)
        {
            user.CompleteOnboarding();
        }

        await repository.AddAsync(user);

        return user;
    }

    // WebApplicationFactory<TEntryPoint> implements both IDisposable and IAsyncDisposable — xunit's
    // IClassFixture support prefers the async path when a fixture implements IAsyncDisposable, so
    // DisposeAsync (not Dispose(bool)) is what actually runs at teardown for these tests. Both are
    // overridden so the database is dropped whichever path a caller takes; each is independently safe
    // to call (EnsureDeleted[Async] is a no-op if the database is already gone).
    public override async ValueTask DisposeAsync()
    {
        await DropDatabaseBestEffortAsync();
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DropDatabaseBestEffort();
        }

        base.Dispose(disposing);
    }

    // See LevelUpWebApplicationFactory.DropDatabaseBestEffort for why this retries: SQL Server LocalDB
    // refuses to DROP DATABASE while this same process still holds a pooled connection to it, and that
    // connection isn't always released the instant the last test method returns.
    private async Task DropDatabaseBestEffortAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var scope = Services.CreateScope();
                var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LevelUpDbContext>>();
                await using var context = await contextFactory.CreateDbContextAsync();

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                await context.Database.EnsureDeletedAsync();
                return;
            }
            catch (Exception)
            {
                await Task.Delay(100);
            }
        }
    }

    private void DropDatabaseBestEffort()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var scope = Services.CreateScope();
                var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LevelUpDbContext>>();
                using var context = contextFactory.CreateDbContext();

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                context.Database.EnsureDeleted();
                return;
            }
            catch (Exception)
            {
                Thread.Sleep(100);
            }
        }
    }
}
