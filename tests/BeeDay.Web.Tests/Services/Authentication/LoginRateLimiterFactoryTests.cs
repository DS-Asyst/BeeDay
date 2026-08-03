using System.Net;
using BeeDay.Web.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BeeDay.Web.Tests.Services.Authentication;

public sealed class LoginRateLimiterFactoryTests
{
    [Fact]
    public void IpLimiter_RejectsAfterPermitLimitFromSameIp()
    {
        using var limiter = LoginRateLimiterFactory.CreateIpLimiter();
        var ip = IPAddress.Parse("203.0.113.10");

        for (var attempt = 0; attempt < 10; attempt++)
        {
            using var lease = limiter.AttemptAcquire(CreateContext(ip, $"user{attempt}@beeday.invalid"));
            Assert.True(lease.IsAcquired, $"Attempt {attempt} should have been permitted.");
        }

        using var rejected = limiter.AttemptAcquire(CreateContext(ip, "one-more@beeday.invalid"));
        Assert.False(rejected.IsAcquired);
    }

    [Fact]
    public void IpLimiter_TracksEachIpIndependently()
    {
        using var limiter = LoginRateLimiterFactory.CreateIpLimiter();
        var first = IPAddress.Parse("203.0.113.10");
        var second = IPAddress.Parse("203.0.113.20");

        for (var attempt = 0; attempt < 10; attempt++)
        {
            using var lease = limiter.AttemptAcquire(CreateContext(first, "user@beeday.invalid"));
            Assert.True(lease.IsAcquired);
        }

        using var otherIpLease = limiter.AttemptAcquire(CreateContext(second, "user@beeday.invalid"));
        Assert.True(otherIpLease.IsAcquired);
    }

    [Fact]
    public void EmailLimiter_RejectsAfterPermitLimitForSameNormalizedEmail()
    {
        using var limiter = LoginRateLimiterFactory.CreateEmailLimiter();
        var ip = IPAddress.Parse("203.0.113.10");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var lease = limiter.AttemptAcquire(CreateContext(IPAddress.Parse($"198.51.100.{attempt}"), "Victim@BeeDay.invalid"));
            Assert.True(lease.IsAcquired, $"Attempt {attempt} should have been permitted.");
        }

        using var rejected = limiter.AttemptAcquire(CreateContext(IPAddress.Parse("198.51.100.99"), "victim@beeday.invalid"));
        Assert.False(rejected.IsAcquired);
    }

    [Fact]
    public void ChainedLimiter_RejectsWhenEitherLimiterIsExhausted()
    {
        using var limiter = LoginRateLimiterFactory.Create();
        var ip = IPAddress.Parse("203.0.113.10");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var lease = limiter.AttemptAcquire(CreateContext(ip, "victim@beeday.invalid"));
            Assert.True(lease.IsAcquired, $"Attempt {attempt} should have been permitted.");
        }

        // The email-specific limit (5) is exhausted even though the IP limit (10) is not.
        using var rejected = limiter.AttemptAcquire(CreateContext(ip, "victim@beeday.invalid"));
        Assert.False(rejected.IsAcquired);
    }

    private static DefaultHttpContext CreateContext(IPAddress ip, string email)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = ip;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["email"] = email
        });
        return context;
    }
}
