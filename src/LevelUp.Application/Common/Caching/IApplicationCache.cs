namespace LevelUp.Application.Common.Caching;

public interface IApplicationCache
{
    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan duration,
        CancellationToken cancellationToken);

    public void Remove(string key);
}
