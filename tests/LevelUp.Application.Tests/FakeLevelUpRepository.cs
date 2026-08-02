using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Tests;

/// <summary>
/// Shared in-memory <see cref="ILevelUpRepository"/> fake for handlers not yet migrated to a
/// per-Aggregate persistence contract (Sprint 13.4 — docs/architecture/07-persistence-contracts.md).
/// Consolidates what used to be nine near-identical private nested classes named
/// <c>Repo</c>/<c>Repository</c>/<c>TestRepository</c> across this project (Sprint 13.6). Carries no
/// test-specific convenience methods (e.g. seeding a "current" user) — those stay local to whichever
/// test class needs them, since they vary per scenario and aren't part of the repository contract.
/// </summary>
internal sealed class FakeLevelUpRepository : ILevelUpRepository
{
    public LevelUpData Data { get; } = new();

    public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Data);

    public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
    {
        mutation(Data);
        return Task.CompletedTask;
    }
}
