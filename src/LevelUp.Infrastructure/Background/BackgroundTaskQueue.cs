using System.Threading.Channels;
using LevelUp.Application.Common.Background;

namespace LevelUp.Infrastructure.Background;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue =
        Channel.CreateBounded<Func<CancellationToken, ValueTask>>(
            new BoundedChannelOptions(256)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });

    public ValueTask QueueAsync(
        Func<CancellationToken, ValueTask> workItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        return _queue.Writer.WriteAsync(workItem, cancellationToken);
    }

    internal ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken) =>
        _queue.Reader.ReadAsync(cancellationToken);
}
