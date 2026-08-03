using BeeDay.Infrastructure.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDay.Infrastructure.Tests.Persistence.SqlServer.Repositories;

/// <summary>
/// Creates a uniquely-named, disposable SQL Server LocalDB database per test instance (xunit creates a
/// new instance per test method by default, so "create database → apply migration → run scenario →
/// drop database" happens once per test, never shared). Applies the real <c>InitialCreate</c> migration
/// via <c>Database.MigrateAsync()</c> — not <c>EnsureCreated</c>, which would skip the migration
/// entirely (including the raw-SQL <c>UX_ExperienceEntries_Dedup</c> index) and defeat the point of
/// testing against the actual migration.
/// </summary>
public abstract class EfLocalDbTestBase : IAsyncLifetime
{
    private ServiceProvider serviceProvider = null!;

    // internal, not protected: BeeDayDbContext is itself internal to BeeDay.Infrastructure (with
    // InternalsVisibleTo granted to this test assembly) — a protected member of a public class cannot
    // expose a less-accessible type (CS0053), but an internal member can, and every derived test class
    // lives in this same assembly regardless.
    internal IDbContextFactory<BeeDayDbContext> ContextFactory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var databaseName = $"LevelUp_EfTests_{Guid.NewGuid():N}";
        var connectionString =
            $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        var services = new ServiceCollection();
        services.AddDbContextFactory<BeeDayDbContext>(options => options.UseSqlServer(connectionString));
        serviceProvider = services.BuildServiceProvider();

        ContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();

        await using var context = await ContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using (var context = await ContextFactory.CreateDbContextAsync())
        {
            await context.Database.EnsureDeletedAsync();
        }

        await serviceProvider.DisposeAsync();
    }
}
