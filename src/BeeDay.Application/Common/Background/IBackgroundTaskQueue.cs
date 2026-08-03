namespace LevelUp.Application.Common.Background;

public interface IBackgroundTaskQueue
{
    public ValueTask QueueAsync(
        Func<CancellationToken, ValueTask> workItem,
        CancellationToken cancellationToken = default);
}
