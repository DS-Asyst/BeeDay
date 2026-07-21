using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LevelUp.Infrastructure.Background;

public sealed class BackgroundTaskWorker(
    BackgroundTaskQueue queue,
    ILogger<BackgroundTaskWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await queue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An unhandled error occurred in a background task.");
            }
        }
    }
}
