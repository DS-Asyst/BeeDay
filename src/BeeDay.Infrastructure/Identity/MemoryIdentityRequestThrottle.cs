using System.Collections.Concurrent;
using BeeDay.Application.Common.Identity;

namespace BeeDay.Infrastructure.Identity;

public sealed class MemoryIdentityRequestThrottle(IClock clock) : IIdentityRequestThrottle
{
    // Every Nth call sweeps expired entries instead of every call, so cleanup cost is amortized
    // across many TryAcquire calls rather than paid up front on the hot path (Sprint 18.6 - the
    // dictionary previously grew without bound, one entry per distinct operation/subject pair ever
    // requested, since nothing ever removed a cooldown once it elapsed).
    private const int CleanupInterval = 128;

    private readonly ConcurrentDictionary<string, DateTimeOffset> _requests = new(StringComparer.Ordinal);
    private long _callCount;

    public bool TryAcquire(string operation, string subject, TimeSpan cooldown, out TimeSpan retryAfter)
    {
        var key = $"{operation}:{subject.Trim().ToUpperInvariant()}";
        var now = clock.UtcNow;

        if (Interlocked.Increment(ref _callCount) % CleanupInterval == 0)
        {
            RemoveExpiredEntries(now);
        }

        while (true)
        {
            if (_requests.TryGetValue(key, out var availableAt) && availableAt > now)
            {
                retryAfter = availableAt - now;
                return false;
            }

            var next = now.Add(cooldown);
            if (_requests.TryAdd(key, next) || _requests.TryUpdate(key, next, availableAt))
            {
                retryAfter = TimeSpan.Zero;
                return true;
            }
        }
    }

    // ConcurrentDictionary enumeration is a weakly-consistent snapshot safe to run alongside
    // concurrent readers/writers; the KeyValuePair overload of TryRemove only removes the entry if
    // it still holds the exact value observed here, so a cooldown renewed by another thread between
    // the enumeration and the removal is never dropped.
    private void RemoveExpiredEntries(DateTimeOffset now)
    {
        foreach (var entry in _requests)
        {
            if (entry.Value <= now)
            {
                _requests.TryRemove(entry);
            }
        }
    }
}
