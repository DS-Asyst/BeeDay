using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Tests;

/// <summary>
/// Locks the Sprint 13.3/13.4 persistence-contract boundary: inspects compiled member signatures
/// (not source text) of every interface in <c>Common.Contracts</c> or any feature-scoped
/// <c>*.Contracts</c> namespace, and fails if any parameter or return type exposes
/// <see cref="LevelUpData"/> — directly, through <c>Task&lt;LevelUpData&gt;</c>, or through a
/// delegate like <c>Action&lt;LevelUpData&gt;</c>/<c>Func&lt;LevelUpData, ...&gt;</c>. Mirrors
/// <c>LevelUp.Domain.Tests.DomainAssemblyBoundaryTests</c>'s approach for the Domain/Infrastructure
/// boundary. <see cref="ILevelUpRepository"/> is the one deliberate exception — the legacy
/// whole-document contract, excluded here until Sprint 13.4's final lot removes it.
///
/// Extended in Sprint 13.6 to also guard, across the same contract set: no <c>System.Text.Json</c>
/// type in any signature, no generic repository/unit-of-work abstraction, and that
/// <c>LevelUp.Application</c> itself never references <c>LevelUp.Infrastructure</c>.
/// </summary>
public sealed class PersistenceContractBoundaryTests
{
    [Fact]
    public void PersistenceContracts_NeverExposeLevelUpDataInAnyMemberSignature()
    {
        var contractInterfaces = GetContractInterfaces();
        Assert.NotEmpty(contractInterfaces);

        foreach (var contract in contractInterfaces)
        {
            foreach (var method in contract.GetMethods())
            {
                foreach (var parameter in method.GetParameters())
                {
                    Assert.False(
                        ExposesLevelUpData(parameter.ParameterType),
                        $"{contract.FullName}.{method.Name} exposes LevelUpData through parameter '{parameter.Name}'.");
                }

                Assert.False(
                    ExposesLevelUpData(method.ReturnType),
                    $"{contract.FullName}.{method.Name} exposes LevelUpData through its return type.");
            }
        }
    }

    [Fact]
    public void LegacyLevelUpRepository_IsTheOnlyContractAllowedToReferenceLevelUpData()
    {
        // A change-detector, not a value judgement: if this ever fails because a *new* contract
        // needs LevelUpData, that new contract was designed wrong. If it fails because
        // ILevelUpRepository was finally removed, delete this test as part of that Sprint 13.4
        // final lot instead of updating it.
        var repositoryMethods = typeof(ILevelUpRepository).GetMethods();
        Assert.Contains(repositoryMethods, method => ExposesLevelUpData(method.ReturnType));
    }

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
    public void PersistenceContracts_ContainNoGenericRepositoryOrUnitOfWorkAbstraction()
    {
        foreach (var contract in GetContractInterfaces())
        {
            Assert.False(
                contract.IsGenericTypeDefinition,
                $"{contract.FullName} is a generic repository abstraction (e.g. IRepository<T>) — " +
                "docs/architecture/07-persistence-contracts.md forbids Generic Repository.");

            Assert.False(
                contract.Name.Contains("UnitOfWork", StringComparison.OrdinalIgnoreCase),
                $"{contract.FullName} looks like a Unit of Work abstraction — none has been approved yet " +
                "(docs/architecture/07-persistence-contracts.md §9/§10 documents the need without implementing it).");
        }
    }

    [Fact]
    public void ApplicationAssembly_DoesNotReferenceInfrastructure()
    {
        var referenced = typeof(ILevelUpRepository).Assembly.GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToArray();

        Assert.DoesNotContain("LevelUp.Infrastructure", referenced);
    }

    private static List<Type> GetContractInterfaces() =>
        [.. typeof(ILevelUpRepository).Assembly.GetTypes()
            .Where(type => type.IsInterface)
            .Where(type => type.Namespace is not null &&
                (type.Namespace == "LevelUp.Application.Common.Contracts"
                    || type.Namespace.EndsWith(".Contracts", StringComparison.Ordinal)))
            .Where(type => type != typeof(ILevelUpRepository))];

    private static bool ExposesLevelUpData(Type type)
    {
        if (type == typeof(LevelUpData))
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericArguments().Any(ExposesLevelUpData);
    }

    private static bool ExposesSystemTextJson(Type type)
    {
        if (type.Namespace is not null && type.Namespace.StartsWith("System.Text.Json", StringComparison.Ordinal))
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericArguments().Any(ExposesSystemTextJson);
    }
}
