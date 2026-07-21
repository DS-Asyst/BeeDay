using LevelUp.Application.Common.Caching;
using LevelUp.Application.Common.Events;
using LevelUp.Domain.Events;
using Xunit;

namespace LevelUp.Application.Tests;

public sealed class DomainEventTests
{
    [Fact]
    public void DomainEvent_CapturesIdentityAndTimestamp()
    {
        var domainEvent = new ApplicationActionDomainEvent("CreateHabitCommand", "CreateHabit");

        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.True(domainEvent.OccurredOnUtc <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task CacheHandler_InvalidatesDashboardCache()
    {
        var cache = new TrackingCache();
        var handler = new InvalidateDashboardCacheHandler(cache);
        var notification = new DomainEventNotification(
            new ApplicationActionDomainEvent("UpdateTodoCommand", "UpdateTodo"));

        await handler.Handle(notification, TestContext.Current.CancellationToken);

        Assert.Equal(CacheKeys.Dashboard, cache.RemovedKey);
    }

    private sealed class TrackingCache : IApplicationCache
    {
        public string? RemovedKey { get; private set; }

        public Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan duration,
            CancellationToken cancellationToken) => factory(cancellationToken);

        public void Remove(string key) => RemovedKey = key;
    }
}
