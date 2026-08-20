using System.Reflection;
using BeeDay.Domain.Entities;
using Xunit;

namespace BeeDay.Domain.Tests;

/// <summary>
/// Locks the Sprint 12.8 boundary: BeeDay.Domain must never again reference a serialization or
/// persistence technology. Inspects the actual compiled assembly's metadata references — not
/// source text — so a transitive `using` in a single file cannot slip past a string search.
/// </summary>
public sealed class DomainAssemblyBoundaryTests
{
    private static readonly string[] ForbiddenAssemblyNames =
    [
        "System.Text.Json",
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "BeeDay.Application",
        "BeeDay.Infrastructure",
        "BeeDay.Web",
    ];

    [Fact]
    public void DomainAssembly_DoesNotReferenceSerializationOrInfrastructureAssemblies()
    {
        var referenced = typeof(User).Assembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToArray();

        foreach (var forbidden in ForbiddenAssemblyNames)
        {
            Assert.DoesNotContain(forbidden, referenced);
        }
    }

    [Fact]
    public void DomainTypes_CarryNoSystemTextJsonAttributes()
    {
        var domainAssembly = typeof(User).Assembly;

        foreach (var type in domainAssembly.GetTypes())
        {
            var members = type.GetMembers(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var member in members)
            {
                foreach (var attribute in member.GetCustomAttributesData())
                {
                    Assert.DoesNotContain(
                        "System.Text.Json",
                        attribute.AttributeType.FullName,
                        StringComparison.Ordinal);
                }
            }
        }
    }
}
