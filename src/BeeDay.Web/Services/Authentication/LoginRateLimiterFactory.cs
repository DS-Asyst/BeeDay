using System.Threading.RateLimiting;

namespace LevelUp.Web.Services.Authentication;

/// <summary>
/// Builds the brute-force protection for <c>/auth/login</c>: two independent sliding-window
/// limiters chained together, so brute-forcing many emails from one IP and brute-forcing one
/// email from many IPs are both throttled. Neither limiter's rejection reveals whether the
/// attempted email belongs to a real account — both look identical from the outside.
/// </summary>
public static class LoginRateLimiterFactory
{
    public static PartitionedRateLimiter<HttpContext> Create(LoginRateLimiterOptions? options = null)
    {
        options ??= new LoginRateLimiterOptions();
        return PartitionedRateLimiter.CreateChained(CreateIpLimiter(options), CreateEmailLimiter(options));
    }

    public static PartitionedRateLimiter<HttpContext> CreateIpLimiter(LoginRateLimiterOptions? options = null)
    {
        options ??= new LoginRateLimiterOptions();
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = options.IpPermitLimit,
                    Window = options.Window,
                    SegmentsPerWindow = options.SegmentsPerWindow,
                    QueueLimit = 0
                }));
    }

    public static PartitionedRateLimiter<HttpContext> CreateEmailLimiter(LoginRateLimiterOptions? options = null)
    {
        options ??= new LoginRateLimiterOptions();
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.Request.HasFormContentType
                    ? httpContext.Request.Form["email"].ToString().Trim().ToUpperInvariant()
                    : string.Empty,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = options.EmailPermitLimit,
                    Window = options.Window,
                    SegmentsPerWindow = options.SegmentsPerWindow,
                    QueueLimit = 0
                }));
    }
}
