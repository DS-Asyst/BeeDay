using BeeDay.Application.Common.Caching;
using MediatR;

namespace BeeDay.Application.Common.Events;

public sealed class InvalidateDashboardCacheHandler(IApplicationCache cache)
    : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        cache.Remove(CacheKeys.Dashboard);
        return Task.CompletedTask;
    }
}

public static class CacheKeys
{
    public const string Dashboard = "dashboard:beeday-data";
}
