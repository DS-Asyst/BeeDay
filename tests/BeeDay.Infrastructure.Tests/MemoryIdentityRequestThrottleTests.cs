using System.Collections.Concurrent;
using System.Reflection;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Identity;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class MemoryIdentityRequestThrottleTests
{
    [Fact]
    public void TryAcquire_FirstRequestForKey_IsPermitted()
    {
        var throttle = new MemoryIdentityRequestThrottle(new FakeClock(DateTimeOffset.UtcNow));

        var acquired = throttle.TryAcquire(
            "email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out var retryAfter);

        Assert.True(acquired);
        Assert.Equal(TimeSpan.Zero, retryAfter);
    }

    [Fact]
    public void TryAcquire_SameKeyWithinCooldown_IsBlocked()
    {
        var clock = new FakeClock(DateTimeOffset.UtcNow);
        var throttle = new MemoryIdentityRequestThrottle(clock);
        throttle.TryAcquire("email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out _);

        clock.UtcNow += TimeSpan.FromSeconds(30);
        var acquired = throttle.TryAcquire(
            "email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out var retryAfter);

        Assert.False(acquired);
        Assert.Equal(TimeSpan.FromSeconds(30), retryAfter);
    }

    [Fact]
    public void TryAcquire_SameKeyAfterCooldown_IsPermitted()
    {
        var clock = new FakeClock(DateTimeOffset.UtcNow);
        var throttle = new MemoryIdentityRequestThrottle(clock);
        throttle.TryAcquire("email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out _);

        clock.UtcNow += TimeSpan.FromSeconds(60);
        var acquired = throttle.TryAcquire(
            "email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out var retryAfter);

        Assert.True(acquired);
        Assert.Equal(TimeSpan.Zero, retryAfter);
    }

    [Fact]
    public void TryAcquire_DifferentOperationOrSubject_IsIndependent()
    {
        var throttle = new MemoryIdentityRequestThrottle(new FakeClock(DateTimeOffset.UtcNow));
        throttle.TryAcquire("email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out _);

        var differentOperation = throttle.TryAcquire(
            "password-reset", "user@beeday.invalid", TimeSpan.FromSeconds(60), out _);
        var differentSubject = throttle.TryAcquire(
            "email-confirmation", "other@beeday.invalid", TimeSpan.FromSeconds(60), out _);

        Assert.True(differentOperation);
        Assert.True(differentSubject);
    }

    [Fact]
    public async Task TryAcquire_ConcurrentRequestsForSameKey_OnlyOneIsPermitted()
    {
        var throttle = new MemoryIdentityRequestThrottle(new FakeClock(DateTimeOffset.UtcNow));
        using var barrier = new Barrier(20);

        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(attempt => Task.Run(() =>
        {
            barrier.SignalAndWait();
            return throttle.TryAcquire("email-confirmation", "user@beeday.invalid", TimeSpan.FromSeconds(60), out _);
        })));

        Assert.Equal(1, results.Count(acquired => acquired));
    }

    [Fact]
    public void TryAcquire_AfterEnoughCalls_EventuallyRemovesExpiredEntries()
    {
        var clock = new FakeClock(DateTimeOffset.UtcNow);
        var throttle = new MemoryIdentityRequestThrottle(clock);
        throttle.TryAcquire("email-confirmation", "stale@beeday.invalid", TimeSpan.FromSeconds(1), out _);

        clock.UtcNow += TimeSpan.FromSeconds(2);
        for (var index = 0; index < 130; index++)
        {
            throttle.TryAcquire("email-confirmation", $"filler{index}@beeday.invalid", TimeSpan.FromSeconds(60), out _);
        }

        Assert.DoesNotContain(
            "email-confirmation:STALE@BEEDAY.INVALID",
            GetRequestKeys(throttle));
    }

    // Reflection is the only way to observe that the amortized cleanup actually ran: the public
    // contract (IIdentityRequestThrottle.TryAcquire) deliberately exposes no read/inspection API,
    // and adding one just for this test would grow the production surface beyond what Sprint 18.6
    // approved.
    private static IEnumerable<string> GetRequestKeys(MemoryIdentityRequestThrottle throttle)
    {
        var field = typeof(MemoryIdentityRequestThrottle)
            .GetField("_requests", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dictionary = (ConcurrentDictionary<string, DateTimeOffset>)field.GetValue(throttle)!;
        return dictionary.Keys;
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;
    }
}
