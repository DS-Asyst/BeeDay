namespace BeeDay.Web.Services.Authentication;

/// <summary>
/// Tunable limits for <see cref="LoginRateLimiterFactory"/>. Defaults match production; bound
/// from configuration (section <c>BeeDay:RateLimiting:Login</c>) so integration tests can use a
/// short window instead of waiting on a real minute, without changing what ships to production.
/// </summary>
public sealed class LoginRateLimiterOptions
{
    public const string SectionName = "BeeDay:RateLimiting:Login";

    public int IpPermitLimit { get; set; } = 10;
    public int EmailPermitLimit { get; set; } = 5;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public int SegmentsPerWindow { get; set; } = 4;
}
