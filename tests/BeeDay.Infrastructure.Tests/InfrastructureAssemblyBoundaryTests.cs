using BeeDay.Infrastructure.Persistence.SqlServer;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

/// <summary>
/// EPIC 30 Sprint 30.9 (BD30-F009): Domain and Application each already had a real assembly-boundary
/// guard (<c>DomainAssemblyBoundaryTests</c>, <c>PersistenceContractBoundaryTests.
/// ApplicationAssembly_DoesNotReferenceInfrastructure</c>) — Infrastructure and Web had none.
/// Inspects the actual compiled assembly's metadata references, not source text, so a transitive
/// <c>using</c> in a single file cannot slip past a string search.
/// </summary>
public sealed class InfrastructureAssemblyBoundaryTests
{
    // BeeDayDbContext is internal, so it is the only type in this assembly this test project can
    // reference to obtain the compiled BeeDay.Infrastructure.dll.
    private static readonly string[] ForbiddenAssemblyNames =
    [
        "BeeDay.Web",
        "Microsoft.AspNetCore.Components",
        "Microsoft.AspNetCore.Components.Web",
        "Microsoft.AspNetCore.Components.Server",
    ];

    [Fact]
    public void InfrastructureAssembly_DoesNotReferenceWebOrBlazorAssemblies()
    {
        var referenced = typeof(BeeDayDbContext).Assembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToArray();

        foreach (var forbidden in ForbiddenAssemblyNames)
        {
            Assert.DoesNotContain(forbidden, referenced);
        }
    }
}
