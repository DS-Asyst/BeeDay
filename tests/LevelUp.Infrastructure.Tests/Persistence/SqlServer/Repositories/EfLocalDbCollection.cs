using Xunit;

namespace LevelUp.Infrastructure.Tests.Persistence.SqlServer.Repositories;

/// <summary>
/// Every Ef*RepositoryTests class creates and drops its own real LocalDB database (see
/// <see cref="EfLocalDbTestBase"/>). Running them in parallel would issue many concurrent
/// CREATE DATABASE/DROP DATABASE statements against the same `mssqllocaldb` instance — the same
/// resource-contention problem already solved for Playwright in LevelUp.E2E.Tests via
/// `DisableTestParallelization`, scoped here to just this collection so the in-memory-only
/// `LevelUpDbContextTests` is unaffected and keeps running in parallel.
/// </summary>
[CollectionDefinition("EfLocalDb", DisableParallelization = true)]
public sealed class EfLocalDbCollection;
