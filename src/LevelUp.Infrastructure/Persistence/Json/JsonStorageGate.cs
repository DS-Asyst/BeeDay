namespace LevelUp.Infrastructure.Persistence.Json;

/// <summary>
/// Coordinates the complete read-modify-write cycle for the configured JSON store.
/// Registered as a singleton so every repository instance in the process shares the same gate.
/// </summary>
public sealed class JsonStorageGate : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await gate.WaitAsync(cancellationToken);
        try
        {
            await operation(cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await operation(cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();
}
