using LevelUp.Application.Common.Identity;

namespace LevelUp.Infrastructure.Identity;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
