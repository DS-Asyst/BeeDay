namespace LevelUp.Application.Common.Identity;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
