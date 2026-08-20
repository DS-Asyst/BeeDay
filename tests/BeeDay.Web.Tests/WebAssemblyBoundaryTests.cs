using BeeDay.Web.Services;

namespace BeeDay.Web.Tests;

/// <summary>
/// EPIC 30 Sprint 30.9 (BD30-F009): Domain and Application each already had a real assembly-boundary
/// guard (<c>DomainAssemblyBoundaryTests</c>, <c>PersistenceContractBoundaryTests.
/// ApplicationAssembly_DoesNotReferenceInfrastructure</c>) — Infrastructure and Web had none.
/// Inspects the actual compiled assembly's metadata references, not source text: Web must reach
/// persistence exclusively through <c>BeeDay.Infrastructure</c>'s public contracts (registered via
/// <c>AddBeeDayInfrastructure</c> in Program.cs), never by referencing EF Core or a SQL Server
/// client directly — the sole confirmed absence of direct <c>BeeDayDbContext</c> access (INV-007)
/// is now backed by an automated guard instead of only a one-time manual grep.
/// </summary>
public sealed class WebAssemblyBoundaryTests
{
    private static readonly string[] ForbiddenAssemblyNames =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.Relational",
        "Microsoft.EntityFrameworkCore.SqlServer",
        "Microsoft.Data.SqlClient",
    ];

    [Fact]
    public void WebAssembly_DoesNotReferenceEntityFrameworkCoreOrSqlServerAssembliesDirectly()
    {
        var referenced = typeof(BeeDayWebService).Assembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToArray();

        foreach (var forbidden in ForbiddenAssemblyNames)
        {
            Assert.DoesNotContain(forbidden, referenced);
        }
    }
}
