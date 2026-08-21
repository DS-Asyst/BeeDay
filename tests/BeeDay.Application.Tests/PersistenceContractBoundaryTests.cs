using BeeDay.Application.Common.Contracts;

namespace BeeDay.Application.Tests;

/// <summary>
/// Locks the Sprint 13.3/13.4 persistence-contract boundary: inspects compiled member signatures
/// (not source text) of every interface in <c>Common.Contracts</c> or any feature-scoped
/// <c>*.Contracts</c> namespace. Mirrors <c>BeeDay.Domain.Tests.DomainAssemblyBoundaryTests</c>'s
/// approach for the Domain/Infrastructure boundary. <c>ILevelUpRepository</c> was the one deliberate
/// exception — the legacy whole-document contract — until it was removed on the Sprint 14.6 SQL
/// Server cutover; every contract is checked with no exception from that Sprint onward.
/// <c>LevelUpData</c> itself (the legacy document Aggregate this guard used to check contracts never
/// exposed) was deleted in Sprint 14.7 — there is no longer a document type for any contract to leak.
///
/// Extended in Sprint 13.6 to also guard, across the same contract set: no <c>System.Text.Json</c>
/// type in any signature, no generic repository/unit-of-work abstraction, and that
/// <c>BeeDay.Application</c> itself never references <c>BeeDay.Infrastructure</c>.
///
/// Sprint 14.5 approved and implemented <see cref="IUnitOfWork"/> (see
/// docs/history/persistence-contracts.md §6/§9/§10/§13) — the guard below now allows exactly
/// that one, non-generic type instead of rejecting anything named "UnitOfWork".
/// </summary>
public sealed class PersistenceContractBoundaryTests
{
    [Fact]
    public void PersistenceContracts_NeverExposeAnySystemTextJsonType()
    {
        foreach (var contract in GetContractInterfaces())
        {
            foreach (var method in contract.GetMethods())
            {
                foreach (var parameter in method.GetParameters())
                {
                    Assert.False(
                        ExposesSystemTextJson(parameter.ParameterType),
                        $"{contract.FullName}.{method.Name} exposes a System.Text.Json type through parameter '{parameter.Name}'.");
                }

                Assert.False(
                    ExposesSystemTextJson(method.ReturnType),
                    $"{contract.FullName}.{method.Name} exposes a System.Text.Json type through its return type.");
            }
        }
    }

    [Fact]
    public void PersistenceContracts_ContainNoGenericRepositoryOrUnapprovedUnitOfWorkAbstraction()
    {
        foreach (var contract in GetContractInterfaces())
        {
            Assert.False(
                contract.IsGenericTypeDefinition,
                $"{contract.FullName} is a generic repository abstraction (e.g. IRepository<T>) — " +
                "docs/history/persistence-contracts.md forbids Generic Repository.");

            // IUnitOfWork (Sprint 14.5) is the one deliberately approved Unit of Work abstraction —
            // everything else that merely looks like one (by name) is still rejected, guarding against
            // a second, uncoordinated Unit of Work sneaking in.
            if (contract == typeof(IUnitOfWork))
            {
                continue;
            }

            Assert.False(
                contract.Name.Contains("UnitOfWork", StringComparison.OrdinalIgnoreCase),
                $"{contract.FullName} looks like a second Unit of Work abstraction — only IUnitOfWork " +
                "(Sprint 14.5, docs/history/persistence-contracts.md §9/§10/§13) is approved.");
        }
    }

    [Fact]
    public void ApplicationAssembly_DoesNotReferenceInfrastructure()
    {
        var referenced = typeof(IUnitOfWork).Assembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToArray();

        Assert.DoesNotContain("BeeDay.Infrastructure", referenced);
    }

    private static List<Type> GetContractInterfaces() =>
        [.. typeof(IUnitOfWork).Assembly.GetTypes()
            .Where(type => type.IsInterface)
            .Where(type => type.Namespace is not null &&
                (type.Namespace == "BeeDay.Application.Common.Contracts"
                    || type.Namespace.EndsWith(".Contracts", StringComparison.Ordinal)))];

    private static bool ExposesSystemTextJson(Type type)
    {
        if (type.Namespace is not null && type.Namespace.StartsWith("System.Text.Json", StringComparison.Ordinal))
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericArguments().Any(ExposesSystemTextJson);
    }
}
