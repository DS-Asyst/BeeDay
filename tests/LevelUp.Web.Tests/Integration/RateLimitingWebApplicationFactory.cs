using LevelUp.Web.Services.Authentication;

namespace LevelUp.Web.Tests.Integration;

/// <summary>
/// Used only by <c>RateLimitingIntegrationTests</c>, which deliberately exhausts the login rate
/// limiter. Overrides the generous defaults in <see cref="LevelUpWebApplicationFactory"/> with
/// tight limits and a short window, so exhausting/recovering a limit doesn't require a real
/// production-length (1 minute) wait. Kept as its own class (rather than a config toggle on the
/// shared factory) precisely so no other test class is ever affected by these tight numbers.
/// </summary>
public sealed class RateLimitingWebApplicationFactory : LevelUpWebApplicationFactory
{
    protected override IReadOnlyDictionary<string, string?> RateLimiterConfiguration { get; } = new Dictionary<string, string?>
    {
        [$"{LoginRateLimiterOptions.SectionName}:IpPermitLimit"] = "6",
        [$"{LoginRateLimiterOptions.SectionName}:EmailPermitLimit"] = "3",
        [$"{LoginRateLimiterOptions.SectionName}:Window"] = "00:00:02",
        [$"{LoginRateLimiterOptions.SectionName}:SegmentsPerWindow"] = "2"
    };
}
