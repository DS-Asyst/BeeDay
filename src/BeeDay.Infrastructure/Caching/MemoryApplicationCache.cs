using LevelUp.Application.Common.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace LevelUp.Infrastructure.Caching;

public sealed class MemoryApplicationCache(IMemoryCache memoryCache) : IApplicationCache
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        if (memoryCache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        memoryCache.Set(key, value, duration);
        return value;
    }

    public void Remove(string key) => memoryCache.Remove(key);
}
