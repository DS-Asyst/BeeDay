namespace LevelUp.Application.Common.Identity;

public interface IIdentityRequestThrottle
{
    public bool TryAcquire(string operation, string subject, TimeSpan cooldown, out TimeSpan retryAfter);
}
