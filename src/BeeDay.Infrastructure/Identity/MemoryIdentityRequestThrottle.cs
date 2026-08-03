using System.Collections.Concurrent;
using BeeDay.Application.Common.Identity;

namespace BeeDay.Infrastructure.Identity;

public sealed class MemoryIdentityRequestThrottle(IClock clock) : IIdentityRequestThrottle
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _requests = new(StringComparer.Ordinal);

    public bool TryAcquire(string operation, string subject, TimeSpan cooldown, out TimeSpan retryAfter)
    {
        var key = $"{operation}:{subject.Trim().ToUpperInvariant()}";
        var now = clock.UtcNow;
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
}
