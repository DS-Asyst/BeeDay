using LevelUp.Application.Common.Auditing;
using LevelUp.Application.Common.Background;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LevelUp.Application.Common.Events;

public sealed class AuditDomainEventHandler(
    IBackgroundTaskQueue backgroundTaskQueue,
    IEventJournal eventJournal,
    ILogger<AuditDomainEventHandler> logger) : INotificationHandler<DomainEventNotification>
{
    public async Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        await backgroundTaskQueue.QueueAsync(
            async workerToken =>
            {
                try
                {
                    await eventJournal.AppendAsync(notification.DomainEvent, workerToken);
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Failed to persist domain event {EventId}",
                        notification.DomainEvent.EventId);
                }
            },
            cancellationToken);
    }
}
