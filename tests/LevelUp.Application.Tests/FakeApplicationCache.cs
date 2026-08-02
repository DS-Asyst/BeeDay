using LevelUp.Application.Common.Caching;

namespace LevelUp.Application.Tests;

/// <summary>
/// Shared no-op <see cref="IApplicationCache"/> fake — always calls straight through to the factory,
/// never actually caching. Consolidates two identical private nested classes (Sprint 13.6).
/// </summary>
internal sealed class FakeApplicationCache : IApplicationCache
{
    public Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan duration, CancellationToken cancellationToken) =>
        factory(cancellationToken);

    public void Remove(string key)
    {
    }
}
