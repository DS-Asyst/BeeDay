using LevelUp.Application.Common.Contracts;
using LevelUp.Domain.Entities;

namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Legacy whole-document port, kept only until every Application handler has migrated to a
/// per-Aggregate persistence contract (Sprint 13.4 — see docs/architecture/07-persistence-contracts.md).
/// Delegates entirely to <see cref="JsonLevelUpDocumentStore"/>, the single internal pipeline also
/// used by the new per-Aggregate adapters — this class adds no read/write/backup logic of its own.
/// </summary>
internal sealed class JsonLevelUpRepository(JsonLevelUpDocumentStore store) : ILevelUpRepository
{
    public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) =>
        store.LoadAsync(cancellationToken);

    public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) =>
        store.SaveAsync(data, cancellationToken);

    public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default) =>
        store.MutateAsync(mutation, cancellationToken);
}
