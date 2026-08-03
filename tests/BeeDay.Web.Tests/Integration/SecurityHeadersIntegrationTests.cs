namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Verifies the security response headers this app ACTUALLY sends today, rather than assuming from
/// reading Program.cs. Two sources contribute headers: Program.cs itself (only <c>UseHsts()</c>,
/// Production-only) and ASP.NET Core's Razor Components framework, which — independent of anything
/// this app configures — automatically adds <c>Content-Security-Policy: frame-ancestors 'self'</c>,
/// <c>X-Frame-Options: SAMEORIGIN</c>, and no-cache Cache-Control/Pragma headers to every
/// interactive-server-rendered response. docs/security/01-security-baseline.md lists a (fuller,
/// script-src-covering) CSP as "planejada" (planned, not required this sprint); Referrer-Policy,
/// X-Content-Type-Options, and Permissions-Policy are not claimed as implemented anywhere, and
/// indeed aren't sent — recorded here as current state, not added speculatively.
/// </summary>
/// <remarks>
/// HSTS could not be verified through <see cref="ProductionLikeWebApplicationFactory"/>:
/// <c>HstsMiddleware</c> only adds its header when <c>HttpContext.Request.IsHttps</c> is true, and
/// <c>Microsoft.AspNetCore.Mvc.Testing</c>'s in-memory TestServer never performs a real TLS
/// handshake — an https:// <c>BaseAddress</c> only affects how relative URLs are built, not
/// <c>Request.IsHttps</c>. This was confirmed by direct observation (a captured Production response
/// showed no Strict-Transport-Security header, status 200, everything else present as expected) and
/// is a known limitation of testing HSTS via WebApplicationFactory in general, not a defect in
/// Program.cs — see the final sprint report.
/// </remarks>
public sealed class SecurityHeadersIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task LoginPage_IncludesFrameworkProvidedFrameAncestorsAndClickjackingHeaders()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/login", cancellationToken);

        Assert.True(response.Headers.TryGetValues("Content-Security-Policy", out var csp));
        Assert.Contains(csp, value => value.Contains("frame-ancestors 'self'", StringComparison.Ordinal));

        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var frameOptions));
        Assert.Contains(frameOptions, value => value.Equals("SAMEORIGIN", StringComparison.Ordinal));

        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.NoCache);
    }

    [Fact]
    public async Task LoginPage_DoesNotYetSendReferrerPolicyContentTypeOptionsOrPermissionsPolicy()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/login", cancellationToken);

        // Current, real state — not a requirement. If any of these become part of the security
        // baseline, update this test (and docs/security/01-security-baseline.md) in the same change.
        Assert.False(response.Headers.Contains("Referrer-Policy"));
        Assert.False(response.Headers.Contains("X-Content-Type-Options"));
        Assert.False(response.Headers.Contains("Permissions-Policy"));
    }

    [Fact]
    public async Task Development_ResponseDoesNotIncludeStrictTransportSecurityHeader()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/login", cancellationToken);

        // UseHsts() only runs for non-Development (Program.cs) — this at least confirms it is
        // never sent in Development, the one HSTS-related fact TestServer can verify here.
        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
    }
}
