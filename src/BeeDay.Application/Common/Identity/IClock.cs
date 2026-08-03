namespace BeeDay.Application.Common.Identity;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}
