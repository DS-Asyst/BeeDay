using LevelUp.Application.Common.Caching;
using MediatR;

namespace LevelUp.Application.Common.Events;

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
    public const string Dashboard = "dashboard:levelup-data";
}
