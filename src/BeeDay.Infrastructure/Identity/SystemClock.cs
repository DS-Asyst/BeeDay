using BeeDay.Application.Common.Identity;

namespace BeeDay.Infrastructure.Identity;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
